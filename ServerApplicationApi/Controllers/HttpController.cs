using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using DecoderLibrary;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;
//using NLog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerApplicationApi.Controllers
{
    [Route("api")]
    [ApiController]
    public class HttpController : ControllerBase
    {
        //private Logger _logger;

        public HttpController()
        {
            //this._logger = NLog.LogManager.GetCurrentClassLogger();
        }

        // GET: api/<DecoderController>
        [HttpGet("download")]
        public async Task<IActionResult> GetFile()
        {
            string filePath = Request.QueryString.Value;
            if (filePath == string.Empty)
                return NotFound();

            string[] filePathArr = filePath.Split(@"\");
            filePath = FileCRUD.FindAddressOfMainFolder() + FileCRUD.CLIENT_FILES_LOCATION + filePathArr[filePathArr.Length - 1];
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, FileCRUD.GetContentType(filePath), filePath);
        }

        // POST api/<DecoderController>
        [HttpPost("upload")]
        public async Task<ActionResult> PostForCheckDecoder()
        {
            IFormFile icdFile = Request.Form.Files[0];
            IFormFile decoderFile = Request.Form.Files[1];
            IPAddress remoteIPAddress = Request.HttpContext.Connection.RemoteIpAddress;

            FileCRUD fileCRUD = SaveFile(decoderFile);
            string icdText = FileCRUD.ReadText(icdFile);
            string settingFileText = FileCRUD.ReadText(fileCRUD.SettingFileLoction);

            List<string> checkResult = await CheckParametersIntegrity(icdText, fileCRUD, settingFileText, remoteIPAddress);
            //NLog.LogManager.Shutdown();

            string responseToClient = BuildResonseToClient(checkResult, fileCRUD.DllFileLocation, icdText, fileCRUD.SettingFileLoction, settingFileText);
            if (responseToClient == string.Empty)
                return Ok();
            else
                return BadRequest(responseToClient);
        }

        private async Task<List<string>> CheckParametersIntegrity(string icdText, FileCRUD fileCRUD, string settingFileText, IPAddress remoteIPAddress)
        {
            List<string> checkResult = new List<string>();
            if (fileCRUD.DllFileLocation != string.Empty && icdText != string.Empty && settingFileText != string.Empty && fileCRUD.NgpFileLocation != string.Empty)
            {
                checkResult = await CheckDecoderController(icdText, fileCRUD.DllFileLocation, settingFileText, fileCRUD.NgpFileLocation, remoteIPAddress);
                if (GetParametersFromClient()[0] == string.Empty) // native check
                {
                    try
                    {
                        double sumTimeCheck = 0;
                        for (int i = 0; i < checkResult.Count; i++)
                            sumTimeCheck += double.Parse(checkResult.ToArray()[i]);

                        checkResult = await CalculateTimeCheck(remoteIPAddress, sumTimeCheck / checkResult.Count, icdText);
                    }
                    catch (FormatException ex)
                    {

                    }
                    
                }
            }

            return checkResult;
        }

        private async Task<List<string>> CheckDecoderController(string icdText, string dllFileLocation, string settingFileText, string ngpFileLocation, IPAddress ipAddress)  
        {
            string[] parametersFromClient = GetParametersFromClient();
            string nativeCheck = parametersFromClient[0];

            DecoderCheckManager decoderCheckManager = new DecoderCheckManager(icdText, dllFileLocation, nativeCheck, GetThroughtCheckValue(parametersFromClient[1]), settingFileText, ngpFileLocation, parametersFromClient[2]/*, this._logger*/);
            List<string> checkResult; List<string> timeCheck = new List<string>();

            while (true)
            {
                checkResult = decoderCheckManager.NavigateByIcdType();

                if (checkResult.Count >= 2) // Send check result to client
                    await SendDataInWebSocket(ipAddress, checkResult);
                else if (nativeCheck == string.Empty && timeCheck.Count < 3) // The server is ruuning for estimated time check 
                {
                    try
                    {
                        double checkResultTime = double.Parse(checkResult.ToArray()[0]);
                        timeCheck.Add(checkResult.ToArray()[0]);
                    }
                    catch (System.FormatException)
                    {
                        await CloseWebSocket(ipAddress);
                        break;
                    }               
                }
                else // get to here if the check was finished or had error
                {
                    await CloseWebSocket(ipAddress);
                    break;
                }

                if (nativeCheck == string.Empty && timeCheck.Count == 3)
                {
                    checkResult = timeCheck;
                    break;
                }
            }

            return checkResult;
        }

        private FileCRUD SaveFile(IFormFile decoderFile)
        {
            try
            {
                FileCRUD fileCRUD = new FileCRUD();
                fileCRUD.FileCRUDManager(decoderFile);
                return fileCRUD;
            }
            catch (Exception ex)
            {
                //this._logger.Error(ex, "fail to open your decoder");
                return null;
            }
        }

        /// <summary>
        /// return the native check and throught check that client send
        /// </summary>
        /// <returns></returns>
        private string[] GetParametersFromClient()
        {
            ICollection<string> listKeys = Request.Form.Keys;
            string[] keysFromClient = new string[3];
            listKeys.CopyTo(keysFromClient, 0);
            return keysFromClient;
        }

        private bool GetThroughtCheckValue(string checkWithRandomSets)
        {
            if (checkWithRandomSets == "true")
                return true;
            else
                return false;
        }

        private string BuildResonseToClient(List<string> checkResult, string dllFileLocation, string icdText, string settingFileLocation, string settingFileText)
        {
            if (checkResult.Count == 1)
                return checkResult.ToArray()[0];
            else if (icdText == string.Empty)
                return "Fail to get the content of the ICD file";
            else if (settingFileLocation == null && dllFileLocation == null)
                return "In the ngp file, you don't put all the required files. Open the guideline and read the requierd files in the ngp file.";
            else if (dllFileLocation == null)
                return "You don't upload decoder file";
            else if (settingFileLocation == null)
                return "You don't upload setting file";
            else if (settingFileText == string.Empty)
                return "Fail to get the content of the setting file";
            else
                return string.Empty;
        }

        private async Task<List<string>> CalculateTimeCheck(IPAddress iPAddress, double timeCheck, string icdText)
        {
            List<string> answer = new List<string>();

            try
            {
                TimeCheckCalculation calculationTimeCheck = new TimeCheckCalculation(icdText, GetThroughtCheckValue(GetParametersFromClient()[1]), timeCheck);
                answer = calculationTimeCheck.CalculationCheckTimeManager();
                await SendDataInWebSocket(iPAddress, answer);
                await CloseWebSocket(iPAddress);
            }
            catch (IndexOutOfRangeException ex)
            {
                //this._logger.Error(ex, "You uploaded incorrect ICD file");
                answer.Add("You upload incorrect ICD file");
            }

            return answer;
        }

        private async Task SendDataInWebSocket(IPAddress ipAddress, List<string> listCollection)
        {
            WebSocket webSocket = WebSocketController.UsersConnectDictionary[ipAddress];

            if (listCollection != null)
                await WebSocketController.SendDataToClient<string>(webSocket, listCollection.ToArray());
        }

        private async Task CloseWebSocket(IPAddress ipAddress)
        {
            WebSocket webSocket = WebSocketController.UsersConnectDictionary[ipAddress];

            //await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
            await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            WebSocketController.UsersConnectDictionary.Remove(ipAddress);
        }
    }
}