using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DecoderLibrary;
using System.IO;
//using NLog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerApplicationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        public static Dictionary<IPAddress, WebSocket> UsersConnectDictionary { get; set; }
        const int HISTORY_LAST_CHECK_LENGTH = 6;
        //public static Logger Logger { get; set; }

        enum NavgiatingOptions
        {
            Decoder,
            FilterTable,
            FilterGraph,
            FilterOptions,
            DecoderVersionOptions,
            Login,
            Register,
            GetUsers
        }

        public WebSocketController()
        {
            //Logger = NLog.LogManager.GetCurrentClassLogger();
        }

        // GET: api/<WebSocketController>
        [HttpGet("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                AddUserToConnectList(webSocket);
                await WebSocketManager(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        public void AddUserToConnectList(WebSocket webSocket)
        {
            Dictionary<IPAddress, WebSocket> connectedDictionary = UsersConnectDictionary;

            if (connectedDictionary == null)
                connectedDictionary = new Dictionary<IPAddress, WebSocket>();

            if (!connectedDictionary.ContainsKey(HttpContext.Connection.RemoteIpAddress))
                connectedDictionary.Add(HttpContext.Connection.RemoteIpAddress, webSocket);

            UsersConnectDictionary = connectedDictionary;
        }

        private async Task WebSocketManager(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            string clientMsg = Encoding.Default.GetString(buffer, 0, result.Count);

            if (clientMsg != NavgiatingOptions.Decoder.ToString())
            {
                List<string> dataList = new List<string>();
                int count = GetMessagesCount(clientMsg);

                for (int i = 0; i < count; i++)
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    dataList.Add(Encoding.Default.GetString(buffer, 0, result.Count));
                }

                if (clientMsg == NavgiatingOptions.FilterOptions.ToString())
                    dataList.AddRange(new List<string> { "0", "1", "2", "3" });

                await NavigateWebSocketOptions(clientMsg, webSocket, dataList);

                UsersConnectDictionary.Remove(HttpContext.Connection.RemoteIpAddress);
            }
            else
            {
                while (UsersConnectDictionary.ContainsKey(HttpContext.Connection.RemoteIpAddress)) { }
            }

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
        }

        private async Task NavigateWebSocketOptions(string clientMsg, WebSocket webSocket, List<string> dataList)
        {
            if (clientMsg == NavgiatingOptions.FilterTable.ToString())
                await GetFilterResult(webSocket, dataList);
            if (clientMsg == NavgiatingOptions.FilterGraph.ToString())
                await GetFilterResultGraph(webSocket, dataList);
            else if (clientMsg == NavgiatingOptions.Login.ToString())
                await Login(webSocket, dataList);
            else if (clientMsg == NavgiatingOptions.Register.ToString())
                await Register(webSocket, dataList);
            else if (clientMsg == NavgiatingOptions.FilterOptions.ToString())
                await GetFilterOptionsResult(webSocket, dataList); 
            else if (clientMsg == NavgiatingOptions.DecoderVersionOptions.ToString())
                await GetFilterDecoderVersionOptions(webSocket, dataList);
            else if (clientMsg == NavgiatingOptions.GetUsers.ToString())
                await GetUsersInDatabase(webSocket);
        }

        private async Task GetFilterResult(WebSocket webSocket, List<string> dataList)
        {
            SummaryAlertCollectionFiltering dataFiltering = new SummaryAlertCollectionFiltering();
            SummaryAlertCollectionItem[] filterResultArr = dataFiltering.Filter(dataList.ToArray()[0], dataList.ToArray()[1], dataList.ToArray()[2], dataList.ToArray()[3], dataList.ToArray()[4]).ToArray();
            string[] itemArr;
            await SendDataToClient(webSocket, new string[] { filterResultArr.Length.ToString() });

            for (int i = 0; i < filterResultArr.Length; i++)
            {
                itemArr = new string[] { filterResultArr[i].DecoderName, filterResultArr[i].ClientDecoderLocation, filterResultArr[i].DataLink, filterResultArr[i].Version,
                    filterResultArr[i].DecoderWritingDate.ToString(), filterResultArr[i].CheckDate.ToString(), filterResultArr[i].IcdTypeName, filterResultArr[i].ThroughtCheck, filterResultArr[i].NativeCheck,
                filterResultArr[i].CountProper, filterResultArr[i].CountNotProper, filterResultArr[i].CountNotValid };
                await SendDataToClient(webSocket, itemArr);
            }
        }

        private async Task GetFilterResultGraph(WebSocket webSocket, List<string> dataList)
        {
            SummaryAlertCollectionFiltering dataFiltering = new SummaryAlertCollectionFiltering();
            SummaryAlertCollectionItem[] filterResultArr = dataFiltering.Filter(dataList.ToArray()[0], dataList.ToArray()[1], dataList.ToArray()[2],dataList.ToArray()[3], dataList.ToArray()[4], dataList.ToArray()[5]).ToArray();

            string[] itemArr; int startIndex; 

            if (filterResultArr.Length < HISTORY_LAST_CHECK_LENGTH)
            {
                startIndex = 0;
                await SendDataToClient(webSocket, new string[] { filterResultArr.Length.ToString() });
            }
            else
            {
                startIndex = filterResultArr.Length - HISTORY_LAST_CHECK_LENGTH;
                await SendDataToClient(webSocket, new string[] { HISTORY_LAST_CHECK_LENGTH.ToString() });
            }
            
            for (int i = startIndex; i < filterResultArr.Length; i++)
            {
                if (filterResultArr[i].ProperList != null)
                {
                    itemArr = new string[] { filterResultArr[i].CheckDate.ToString(""), filterResultArr[i].CountProper, filterResultArr[i].CountNotProper, filterResultArr[i].CountNotValid };
                    await SendDataToClient(webSocket, itemArr);

                    for (int j = 0; j < 3; j++)
                    {
                        if (j == 0)
                        {
                            itemArr = new string[] { ResultTypes.Proper.ToString(), filterResultArr[i].ProperList.Count.ToString() };
                            await SendDataToClient(webSocket, itemArr);
                            await SendDataToClient(webSocket, filterResultArr[i].ProperList.ToArray());
                        }
                        else if (j == 1)
                        {
                            await SendDataToClient(webSocket, new string[] { ResultTypes.NotProper.ToString(), filterResultArr[i].NotProperList.Count.ToString() });
                            await SendDataToClient(webSocket, filterResultArr[i].NotProperList.ToArray());
                        }
                        else
                        {
                            await SendDataToClient(webSocket, new string[] { ResultTypes.NotValid.ToString(), filterResultArr[i].NotValidList.Count.ToString() });
                            await SendDataToClient(webSocket, filterResultArr[i].NotValidList.ToArray());
                        }
                    }
                }
            }

            await GetFilterResultGraphMonths(webSocket, dataList, dataFiltering, filterResultArr);
        }

        private async Task GetFilterResultGraphMonths(WebSocket webSocket, List<string> dataList, SummaryAlertCollectionFiltering dataFiltering, SummaryAlertCollectionItem[] filterResultArr)
        {
            Dictionary<string, double> graphMonthDictionary = dataFiltering.CalculateGradeForLastCheckInMonth(filterResultArr);

            string[] itemArr;
            await SendDataToClient(webSocket, new string[] { graphMonthDictionary.Count.ToString() });

            foreach (string date in graphMonthDictionary.Keys)
            {
                itemArr = new string[] { date, graphMonthDictionary[date].ToString() };
                await SendDataToClient(webSocket, itemArr);
            }
        }

        private async Task GetFilterOptionsResult(WebSocket webSocket, List<string> dataList)
        {
            SummaryAlertCollectionFiltering dataFiltering = new SummaryAlertCollectionFiltering();
            for (int i = 0; i < dataList.ToArray().Length - 1; i++)
            {
                string[] filterResultArr = dataFiltering.GetAllOptionsInColumn(dataList.ToArray()[0], int.Parse(dataList.ToArray()[i + 1])).ToArray();
                await SendDataToClient(webSocket, new int[] { filterResultArr.Length });
                await SendDataToClient(webSocket, filterResultArr);
            }
        }

        private async Task GetFilterDecoderVersionOptions(WebSocket webSocket, List<string> dataList)
        {
            SummaryAlertCollectionFiltering dataFiltering = new SummaryAlertCollectionFiltering();
            string[] filterResultArr = dataFiltering.GetDecoderVersionOptions(dataList.ToArray()[0], dataList.ToArray()[1]).ToArray();
            await SendDataToClient(webSocket, new int[] { filterResultArr.Length });
            await SendDataToClient(webSocket, filterResultArr);
        }

        private async Task Register(WebSocket webSocket, List<string> dataList)
        {
            UsersCRUD usersCRUD = new UsersCRUD();
            string result = usersCRUD.AddUser(dataList.ToArray()[0], dataList.ToArray()[1], dataList.ToArray()[2]).ToString();
            await SendDataToClient(webSocket, new string[] { result });
        }

        private async Task Login(WebSocket webSocket, List<string> dataList)
        {
            UsersCRUD usersCRUD = new UsersCRUD();
            Tuple<bool, string> userLogin = usersCRUD.Login(dataList.ToArray()[0], dataList.ToArray()[1]);
            if (userLogin.Item2 != string.Empty)
                await SendDataToClient(webSocket, new string[] { userLogin.Item1.ToString(), userLogin.Item2 });
            else
                await SendDataToClient(webSocket, new string[] { userLogin.Item1.ToString() });
        }

        private async Task GetUsersInDatabase(WebSocket webSocket)
        {
            UsersCRUD usersCRUD = new UsersCRUD();
            List<UserData> usersList = usersCRUD.GetUsersInDatabase();
            await SendDataToClient(webSocket, new string[] { usersList.Count.ToString() });
            for (int i = 0; i < usersList.Count; i++)
                await SendDataToClient(webSocket, new string[] { usersList.ToArray()[i].UserName, 
                    usersList.ToArray()[i].Permission.ToString(), usersList.ToArray()[i].DeoderPermission.ToString() });
        }

        private int GetMessagesCount(string clientMsg)
        {
            if (clientMsg == NavgiatingOptions.Login.ToString() || clientMsg == NavgiatingOptions.DecoderVersionOptions.ToString())
                return 2;
            else if (clientMsg == NavgiatingOptions.Register.ToString())
                return 3;
            else if (clientMsg == NavgiatingOptions.FilterTable.ToString())
                return 5;
            else if (clientMsg == NavgiatingOptions.FilterGraph.ToString())
                return 6;
            else if (clientMsg == NavgiatingOptions.GetUsers.ToString())
                return 0;
            else // FilterOptions
                return 1;
        }

        public static async Task SendDataToClient<T>(WebSocket webSocket, T[] dataArr)
        {
            for (int i = 0; i < dataArr.Length; i++)
            {
                try
                {
                    if (dataArr[i] != null)
                    {
                        byte[] serverMsg = Encoding.UTF8.GetBytes(dataArr[i].ToString());
                        await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), 0, true, CancellationToken.None);
                    }                  
                }
                catch (Exception ex) when (ex is ArgumentNullException || ex is ArgumentNullException || ex is TaskCanceledException)
                {
                    //Logger.Error(ex, "failed send to client");
                    Console.WriteLine("failed send to client");
                    break;
                }
            }
        }
    }
}
