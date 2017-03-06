/*--------------------------------------------------------------------------------------+
//----------------------------------------------------------------------------
// DOCUMENT ID:   
// LIBRARY:       
// CREATOR:       Mark Anderson
// DATE:          05-05-2016
//
// NAME:          SchemaCheckerMenu.cs
//
// DESCRIPTION:   Utility to create translation jobs.
//
// REFERENCES:    ProjectWise.
//
// ---------------------------------------------------------------------------
// NOTICE
//    NOTICE TO ALL PERSONS HAVING ACCESS HERETO:  This document or
//    recording contains computer software or related information
//    constituting proprietary trade secrets of Black & Veatch, which
//    have been maintained in "unpublished" status under the copyright
//    laws, and which are to be treated by all persons having acdcess
//    thereto in manner to preserve the status thereof as legally
//    protectable trade secrets by neither using nor disclosing the
//    same to others except as may be expressly authorized in advance
//    by Black & Veatch.  However, it is intended that all prospective
//    rights under the copyrigtht laws in the event of future
//    "publication" of this work shall also be reserved; for which
//    purpose only, the following is included in this notice, to wit,
//    "(C) COPYRIGHT 1997 BY BLACK & VEATCH, ALL RIGHTS RESERVED"
// ---------------------------------------------------------------------------
/*
/* CHANGE LOG
 * $Archive: /ProjectWise/ASFramework/SchemaCheckerMenu/SchemaCheckerMenu/SchemaCheckerMenu.cs $
 * $Revision: 2 $
 * $Modtime: 3/06/17 11:06a $
 * $History: SchemaCheckerMenu.cs $
 * 
 * *****************  Version 2  *****************
 * User: Mark.anderson Date: 3/06/17    Time: 2:55p
 * Updated in $/ProjectWise/ASFramework/SchemaCheckerMenu/SchemaCheckerMenu
 * 
 * *****************  Version 1  *****************
 * User: Mark.anderson Date: 2/15/17    Time: 7:55a
 * Created in $/ProjectWise/ASFramework/SchemaCheckerMenu/SchemaCheckerMenu
 * A sample Menu to run the schema checker in the automation services
 * framework.
 * 
*/
using System;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Data;
using System.Windows.Forms;
using System.Net;
using RGiesecke.DllExport;
using System.Collections.Specialized;

namespace SchemaCheckerMenu
{
    /// <summary>
    /// a class that implements the callbacks for a ProjectWise menu.  This should
    /// be used as a template for menu implementation for an AS client.  The
    /// menu calls a webservice which then calls the runjob exe to load up 
    /// a service job.  The class depends on the DLLExport utility to export the
    /// functions for the menu builder.
    /// </summary>
    public class SchemaCheckerMenu
    {
        const int IDOK = 1;
        const int IDCANCEL = 2;
        const int IDABORT = 3;
        const int SETTING_ID = -12471;
        /// <summary>
        /// This will process documents selected in PWC  it will max out the group at 10 docs per job.
        /// </summary>
        /// <param name="uiCount"> number of documents selected.</param>
        /// <param name="plProjArray">the array of  project ids</param>
        /// <param name="plDocArray">the array of  document ids</param>
        /// <returns>IDOK</returns>
        [DllExport]
        public static int SCDocumentCommand
        (
            uint uiCount, //==>Count of documents 
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]int[] plProjArray, //==>Project number Array
            [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]int[] plDocArray //==> Document number Array
        )
        {
            string sURL = PWWrapper.GetPWStringSetting(SETTING_ID);
            StringBuilder outputName = new StringBuilder(512);
            if (!string.IsNullOrEmpty(sURL))
            {
                PWWrapper.aaApi_GetDocumentFileName(plProjArray[0], plDocArray[0], outputName, 512);

                StringBuilder docList = new StringBuilder();
                long groupFactor = 10;
                long numGroups = uiCount / groupFactor;

                for (int g = 0; g <= numGroups; ++g)
                {
                    docList = new StringBuilder();
                    long removeEntries = uiCount - groupFactor * (g + 1);
                    long r = 0;
                    if (removeEntries < 0)
                        r = removeEntries;

                    for (long i = 0 + (g * groupFactor); i < (int)groupFactor * (g + 1) + r; i++)
                    {//don't add dead docs to the list...
                        if (1 == PWWrapper.aaApi_SelectDocument(plProjArray[i], plDocArray[i]))
                            docList.Append(string.Format("{0}:{1} ", plProjArray[i], plDocArray[i]));
                    }

                    StringBuilder dataSourceName = new StringBuilder(1024);
                    StringBuilder loginName = new StringBuilder(512);
                    StringBuilder pwd = new StringBuilder(1024);
                    StringBuilder schemaName = new StringBuilder(1024);
                    IntPtr hData = new IntPtr();
                    int lptype = 0;
                    int lpLoginType = 0;
                    bool pODBC = false;
                    //PWWrapper.aaApi_GetActiveDatasourceName(dataSourceName, 1024);
                    PWWrapper.aaApi_GetConnectionInfo(hData, ref pODBC, ref lptype, ref lpLoginType, dataSourceName, 1024, loginName, 512, pwd, 1024, schemaName, 1024);
                    if (docList.Length > 0)
                        try
                        {
                            PWWrapper.aaApi_ShowInfoMessage(string.Format("Processing {0} File(s)", uiCount));
                            using (var client = new WebClient())
                            {
                                StringBuilder docSet = new StringBuilder(string.Format("{0}", docList.ToString()));

                               // if (dataSourceName.Length > 1)
                            
                                //encode the URL parameters...
                                NameValueCollection dataPairs = new NameValueCollection();
                                dataPairs.Add("docset", docSet.ToString());
                                dataPairs.Add("appName","BVSchemaChecker");
                                dataPairs.Add("AppKeyin","\"BVSchemaChecker Check\"");
                                dataPairs.Add("pwdatasourcename", dataSourceName.ToString());
                                dataPairs.Add("pwLoginCMD", "sheetautomation login");
                                dataPairs.Add("pwusername",loginName.ToString());
                                dataPairs.Add("pwpwd",pwd.ToString());
                                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                                byte[] response = client.UploadValues(sURL + "HPEDocProcessorWebService/HPEDocProcessorWebService.asmx/RunJobEx", "POST", dataPairs);
                                string responseJson = Encoding.UTF8.GetString(response);

                            }

                            PWWrapper.aaApi_ShowInfoMessage("");
                        }
                        catch (Exception ex)
                        {
                            BPSUtilities.WriteLog(ex.Message);
                            BPSUtilities.WriteLog(ex.StackTrace);
                        }

                    PWWrapper.aaApi_UpdateDocumentWindows();
                }
            }
            else
            {
                MessageBox.Show("Set web service address first");
            }

            return IDOK;
        }
        /// <summary>
        /// process all the files in a folder.  This will be visible on the context menu when the user
        /// has selected a folder.   
        /// </summary>
        /// <param name="iProjectId"></param>
        /// <returns></returns>
        [DllExport]
        public static int ProjectCommand
        (
            int iProjectId //==>Selected folder 
        )
        {
            string sURL = PWWrapper.GetPWStringSetting(SETTING_ID);

            if (!string.IsNullOrEmpty(sURL))
            {
                StringBuilder dataSourceName = new StringBuilder(1024);
                StringBuilder loginName = new StringBuilder(512);
                StringBuilder pwd = new StringBuilder(1024);
                StringBuilder schemaName = new StringBuilder(1024);
                IntPtr hData = new IntPtr();
                int lptype = 0;
                int lpLoginType = 0;
                bool pODBC = false;
                //PWWrapper.aaApi_GetActiveDatasourceName(dataSourceName, 1024);
                PWWrapper.aaApi_GetConnectionInfo(hData, ref pODBC, ref lptype, ref lpLoginType, dataSourceName, 1024, loginName, 512, pwd, 1024, schemaName, 1024);

                try
                {
                    PWWrapper.aaApi_ShowInfoMessage("");
                    PWWrapper.aaApi_ShowInfoMessage(string.Format("Processing '{0}'", PWWrapper.GetProjectNamePath(iProjectId)));


                    int docCount = PWWrapper.aaApi_SelectDocumentsByProjectId(iProjectId);
                    if (docCount <= 0)
                        return docCount;
                    for (int i = 0; i < docCount; i++)
                    {
                        int docID = PWWrapper.aaApi_GetDocumentId(i);
                        //now i have the doc and project id build the url and send it...
                        PWWrapper.aaApi_ShowInfoMessage(string.Format("Processing {0} File(s)", i));
                        using (var client = new WebClient())
                        {
                            StringBuilder docSet = new StringBuilder(string.Format("{0}:{1}",docID,iProjectId ));

                            //encode the URL parameters...
                            NameValueCollection dataPairs = new NameValueCollection();
                            dataPairs.Add("docset", docSet.ToString());
                            dataPairs.Add("appName", "BVSchemaChecker");
                            dataPairs.Add("AppKeyin", "\"BVSchemaChecker Check\"");
                            dataPairs.Add("pwdatasourcename", dataSourceName.ToString());
                            dataPairs.Add("pwLoginCMD", "sheetautomation login");
                            dataPairs.Add("pwusername", loginName.ToString());
                            dataPairs.Add("pwpwd", pwd.ToString());
                            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                            byte[] response = client.UploadValues(sURL + "HPEDocProcessorWebService/HPEDocProcessorWebService.asmx/RunJobEx", "POST", dataPairs);
                            string responseJson = Encoding.UTF8.GetString(response);

                        }

                        PWWrapper.aaApi_ShowInfoMessage("");
                    }
                        
                }
                catch (Exception ex)
                {
                    BPSUtilities.WriteLog(ex.Message);
                    BPSUtilities.WriteLog(ex.StackTrace);
                }

                PWWrapper.aaApi_UpdateDocumentWindows();
            }
            else
            {
                MessageBox.Show("Set web service address first");
            }

            return IDOK;
        }
        /// <summary>
        /// used to set the webservice URL.  This will be added to the Tools menu.   When the user has privilages 
        /// they can set the URL for the webservice that will call the translation job creator.
        /// this should be the host URL  http://localhost is typical for testing else
        /// it is the PW Integration server.
        /// </summary>
        /// <returns></returns>
        [DllExport]
        public static int GenericCommand
        (
        )
        {
            string sURL = PWWrapper.GetPWStringSetting(SETTING_ID);

            GetURLDlg dlg = new GetURLDlg();

            dlg.URL = sURL;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrEmpty(dlg.URL))
                    PWWrapper.SetPWStringSetting(SETTING_ID, dlg.URL);
            }

            return IDOK;
        }
        /// <summary>
        /// this checks to see if the current user is an Admin.  If so then
        /// the user is allowed to change the URL.
        /// </summary>
        /// <param name="uiCmdId"></param>
        /// <param name="ulState"></param>
        /// <returns></returns>
        [DllExport]
        public static PWWrapper.MenuItemStateFlag MenuStateFunction
        (
          uint uiCmdId,  // command identifier
          uint ulState  // state mask
        )
        {
            if (PWWrapper.aaApi_IsCurrentUserAdmin())
                return PWWrapper.MenuItemStateFlag.Show;

            return PWWrapper.MenuItemStateFlag.GrayedOut;
            // return (int)PWWrapper.MenuItemStateFlag.Hidden;
        }
    }
}
