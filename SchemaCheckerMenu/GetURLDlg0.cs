/*--------------------------------------------------------------------------------------+
//----------------------------------------------------------------------------
// DOCUMENT ID:   
// LIBRARY:       
// CREATOR:       Mark Anderson
// DATE:          05-05-2016
//
// NAME:          GetURLDlg.cs
//
// DESCRIPTION:   A managed menu extension for ProjectWise Explorer.
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
 * $Archive: /ProjectWise/ASFramework/SchemaCheckerMenu/SchemaCheckerMenu/GetURLDlg0.cs $
 * $Revision: 1 $
 * $Modtime: 2/13/17 2:45p $
 * $History: GetURLDlg0.cs $
 * 
 * *****************  Version 1  *****************
 * User: Mark.anderson Date: 2/15/17    Time: 7:54a
 * Created in $/ProjectWise/ASFramework/SchemaCheckerMenu/SchemaCheckerMenu
 * A sample Menu to run the schema checker in the automation services
 * framework.
 * 
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SchemaCheckerMenu
{
    public partial class GetURLDlg : Form
    {

        public GetURLDlg()
        {

            InitializeComponent();

        }

        public string URL { get; set; }

        private void GetURLDlg_Load(object sender, EventArgs e)
        {
            textBoxURL.Text = URL;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            URL = textBoxURL.Text;
            this.Close();
        }
    }
}
