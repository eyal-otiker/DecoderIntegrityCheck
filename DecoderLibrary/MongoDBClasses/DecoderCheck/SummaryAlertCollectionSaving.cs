using System;
using System.Collections.Generic;

namespace DecoderLibrary
{
    public class SummaryAlertCollectionSaving
    {
        private const string connectionString = "mongodb://localhost:27017";
        private const string databaseCheckResultName = "DecoderCheckData";

        public void SaveData(string countProper, string countNotProper, string countNotValid, string typeOfIcdItem, string nativeCheck, bool isRandomCheck, string clientDecoderLocation, ClientDecoderDetail clientDecoderDetail, string userName, List<string> properList, List<string> notProperList, List<string> notValidList)
        {
            IDatabaseContext databaseContext = new DatabaseContext(connectionString, databaseCheckResultName);
            ISummaryAlertCollectionRepository testObjRepository = new SummaryAlertCollectionRepository(databaseContext);

            testObjRepository.Add(new SummaryAlertCollectionItem
            {
                UserName = userName,
                DecoderName = clientDecoderDetail.DecoderName,
                ClientDecoderLocation = clientDecoderLocation,
                DataLink = clientDecoderDetail.DataLink.ToString(),
                Version = clientDecoderDetail.Version,
                DecoderWritingDate = clientDecoderDetail.DecoderWritingDate,
                CheckDate = DateTime.Now,
                IcdTypeName = typeOfIcdItem,
                NativeCheck = nativeCheck,
                ThroughtCheck = DecriptionThroughtCheck(isRandomCheck),
                CountProper = countProper,
                CountNotProper = countNotProper,
                CountNotValid = countNotValid,
                ProperList = properList,
                NotProperList = notProperList,
                NotValidList = notValidList
            }) ; ; ;
        }

        private string DecriptionThroughtCheck(bool isCheckWithRandom)
        {
            if (isCheckWithRandom)
                return "Random Sets";
            else
                return "Serial Sets";
        }

    }
}
