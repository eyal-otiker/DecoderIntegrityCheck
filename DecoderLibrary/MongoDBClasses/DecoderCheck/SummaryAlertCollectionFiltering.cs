using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DecoderLibrary
{
    public enum ResultTypes
    {
        Proper,
        NotProper,
        NotValid
    }

    enum GetDataOptions
    {
        GetDecoderNames,
        GetIcdTypes,
        GetThroughtChecks,
        GetNativeChecks
    }

    public class SummaryAlertCollectionFiltering
    {
        private string connectionString = "mongodb://localhost:27017";
        private string databaseCheckResultName = "DecoderCheckData";
        const int GRAPH_MONTH_COUNT = 12;

        public SummaryAlertCollectionFiltering() 
        {
            
        }

        public ISummaryAlertCollectionRepository ConnectToData()
        {
            IDatabaseContext databaseContext = new DatabaseContext(connectionString, databaseCheckResultName);
            ISummaryAlertCollectionRepository summaryCollectionRepository = new SummaryAlertCollectionRepository(databaseContext);
            return summaryCollectionRepository;
        }

        public List<SummaryAlertCollectionItem> Filter(string userName, string decoderName, string icdType, string throughtCheck, string nativeCheck, string decoderVersion = "")
        {
            ISummaryAlertCollectionRepository summaryCollectionRepository = ConnectToData();
            FilterDefinition<SummaryAlertCollectionItem> filters = BuildFilterDefinition(decoderName, icdType, throughtCheck, nativeCheck, decoderVersion);

            List<SummaryAlertCollectionItem> summaryCollectionList = summaryCollectionRepository.GetCollection().Find(filters).ToList();
            List<SummaryAlertCollectionItem> summaryCollectionListReturn = new List<SummaryAlertCollectionItem>();

            foreach (SummaryAlertCollectionItem summaryAlertCollectionItem in summaryCollectionList)
            {
                if (CanViewDecoder(userName, summaryAlertCollectionItem.UserName) == true)
                    summaryCollectionListReturn.Add(summaryAlertCollectionItem);
            }

            return summaryCollectionListReturn;
        }

        public Dictionary<string, double> CalculateGradeForLastCheckInMonth(SummaryAlertCollectionItem[] filterArr)
        {
            Dictionary<string, double> gradeGraphDictionary = new Dictionary<string, double>();
            double grade; string addZero;

            for (int i = 1; i <= filterArr.Length; i++)
            {
                if (i == filterArr.Length || filterArr[i].CheckDate.Date.Month != filterArr[i - 1].CheckDate.Date.Month)
                {
                    //if (filterArr[i - 1].NotProperList.Count == 0 && filterArr[i - 1].NotValidList.Count == 0)
                    //    grade = 100;                  
                    //else
                    //    grade = 2 * double.Parse(filterArr[i - 1].CountProper.Replace('%', ' ')) /
                    //    (7 * double.Parse(filterArr[i - 1].CountNotProper.Replace('%', ' ')) +
                    //    15 * double.Parse(filterArr[i - 1].CountNotValid.Replace('%', ' '))) * 100;

                    grade = double.Parse(filterArr[i - 1].CountProper.Replace('%', ' ')) * 1 +
                        double.Parse(filterArr[i - 1].CountNotProper.Replace('%', ' ')) * 0.5 +
                        double.Parse(filterArr[i - 1].CountNotValid.Replace('%', ' ')) * 0;
                    grade = Math.Round(grade, 2);
                    if (!gradeGraphDictionary.ContainsKey(filterArr[i - 1].CheckDate.Date.Month.ToString() + "/" + filterArr[i - 1].CheckDate.Date.Year.ToString()))
                    {
                        if (filterArr[i - 1].CheckDate.Date.Month < 10)
                            addZero = "0";
                        else
                            addZero = "";
                        gradeGraphDictionary.Add(addZero + filterArr[i - 1].CheckDate.Date.Month.ToString("") + "/" + filterArr[i - 1].CheckDate.Date.Year.ToString(), grade);
                    }
                }
            }

            if (gradeGraphDictionary.Count > GRAPH_MONTH_COUNT)
                gradeGraphDictionary = FixGradeGraphDictionaryCount(gradeGraphDictionary);

            return gradeGraphDictionary;
        }

        private Dictionary<string, double> FixGradeGraphDictionaryCount(Dictionary<string, double> gradeGraphDictionary)
        {
            foreach (string date in gradeGraphDictionary.Keys)
            {
                gradeGraphDictionary.Remove(date);
                if (gradeGraphDictionary.Count == GRAPH_MONTH_COUNT)
                    break;
            }
            return gradeGraphDictionary;
        }

        private FilterDefinition<SummaryAlertCollectionItem> BuildFilterDefinition(string decoderName, string icdType, string throughtCheck, string nativeCheck, string decoderVersion)
        {
            FilterDefinition<SummaryAlertCollectionItem> filter = Builders<SummaryAlertCollectionItem>.Filter.Empty;

            if (decoderName != string.Empty)
                filter &= Builders<SummaryAlertCollectionItem>.Filter.Eq(item => item.DecoderName, decoderName);
            if (icdType != string.Empty)
                filter &= Builders<SummaryAlertCollectionItem>.Filter.Eq(item => item.IcdTypeName, icdType);
            if (throughtCheck != string.Empty)
                filter &= Builders<SummaryAlertCollectionItem>.Filter.Eq(item => item.ThroughtCheck, throughtCheck);
            if (nativeCheck != string.Empty)
                filter &= Builders<SummaryAlertCollectionItem>.Filter.Eq(item => item.NativeCheck, nativeCheck);
            if (decoderVersion != string.Empty)
                filter &= Builders<SummaryAlertCollectionItem>.Filter.Eq(item => item.Version, decoderVersion); 

            return filter;
        }

        public List<string> GetAllOptionsInColumn(string userName, int taskNumber)
        {
            ISummaryAlertCollectionRepository summaryCollectionRepository = ConnectToData();

            List<SummaryAlertCollectionItem> summaryCollectionList = summaryCollectionRepository.GetCollection().Find(Builders<SummaryAlertCollectionItem>.Filter.Empty).ToList();

            List<string> listNames = new List<string>();
            foreach (SummaryAlertCollectionItem summaryAlertCollectionItem in summaryCollectionList)
            {
                if (CanViewDecoder(userName, summaryAlertCollectionItem.UserName) == true)
                {
                    if (taskNumber == (int)GetDataOptions.GetDecoderNames)
                    {
                        if (!listNames.Contains(summaryAlertCollectionItem.DecoderName))
                            listNames.Add(summaryAlertCollectionItem.DecoderName);
                    }
                    else if (taskNumber == (int)GetDataOptions.GetIcdTypes)
                    {
                        if (!listNames.Contains(summaryAlertCollectionItem.IcdTypeName))
                            listNames.Add(summaryAlertCollectionItem.IcdTypeName);
                    }
                    else if (taskNumber == (int)GetDataOptions.GetThroughtChecks)
                    {
                        if (!listNames.Contains(summaryAlertCollectionItem.ThroughtCheck))
                            listNames.Add(summaryAlertCollectionItem.ThroughtCheck);
                    }
                    else if (taskNumber == (int)GetDataOptions.GetNativeChecks)
                    {
                        if (!listNames.Contains(summaryAlertCollectionItem.NativeCheck))
                            listNames.Add(summaryAlertCollectionItem.NativeCheck);
                    }
                }                
            }               

            return listNames;
        }

        public List<string> GetDecoderVersionOptions(string userName, string decoderName)
        {
            ISummaryAlertCollectionRepository summaryCollectionRepository = ConnectToData();
            FilterDefinition<SummaryAlertCollectionItem> filter = Builders<SummaryAlertCollectionItem>.Filter.Eq(item => item.DecoderName, decoderName);
            List<SummaryAlertCollectionItem> summaryCollectionList = summaryCollectionRepository.GetCollection().Find(filter).ToList();

            List<string> decoderVersionsList = new List<string>();
            foreach (SummaryAlertCollectionItem summaryAlertCollectionItem in summaryCollectionList)
            {
                if (CanViewDecoder(userName, summaryAlertCollectionItem.UserName) && !decoderVersionsList.Contains(summaryAlertCollectionItem.Version))
                    decoderVersionsList.Add(summaryAlertCollectionItem.Version);
            }
            return decoderVersionsList;
        }

        private bool CanViewDecoder(string currentUserName, string checkUserName)
        {
            UsersCRUD usersCRUD = new UsersCRUD();
            DecoderPermission decoderCheckUserName = usersCRUD.GetDecoderPermissionForUser(checkUserName);
            Permission currentUserNamePermission = usersCRUD.GetUserPermission(currentUserName);

            if (decoderCheckUserName == DecoderPermission.EveryOne || currentUserName == checkUserName ||
                (currentUserNamePermission == Permission.Admin && decoderCheckUserName == DecoderPermission.YouAndAdmin))
                return true;

            return false;
        }
    }
}
