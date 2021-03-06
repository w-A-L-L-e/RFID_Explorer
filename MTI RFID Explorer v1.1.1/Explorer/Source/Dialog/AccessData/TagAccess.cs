/*
 *****************************************************************************
 *                                                                           *
 *                   MTI CONFIDENTIAL AND PROPRIETARY                        *
 *                                                                           *
 * This source code is the sole property of MTI, Inc.  Reproduction or       *
 * utilization of this source code in whole or in part is forbidden without  *
 * the prior written consent of MTI, Inc.                                    *
 *                                                                           *
 * (c) Copyright MTI, Inc. 2011. All rights reserved.                        *
 *                                                                           *
 *****************************************************************************
 */

/*
 *****************************************************************************
 *
 * $Id: TagAccess.cs,v 1.6 2009/12/16 23:38:22 dciampi Exp $
 * 
 * Description: 
 *
 *****************************************************************************
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Globalization;

using rfid.Constants;
using RFID.RFIDInterface;

namespace RFID_Explorer
{
    public partial class FORM_TagAccess : Form
	{                                   
        private static TagAccessData _tagAccessData;
        private static TagAccessReads _tagAccessReads;//??加參數??//Add LargeRead command

        private LakeChabotReader _reader = null;
        public TagAccessReads TagAccessReadSet//??加參數??//Add LargeRead command
        {
            get { return _tagAccessReads; }
            set { _tagAccessReads = value; }
        }

        public TagAccessData TagAccessDataSet
        {
            get { return _tagAccessData; }
            set { _tagAccessData = value; }
        }

        public void Init(LakeChabotReader reader)
        {
            InitializeComponent();

            //clark 2011.5.9 Get this point to get Access Password in module's buffer.
            _reader = reader;

            //clark not sure. Hide QT function. Wait FW team to support.
            //this.COMBOBOX_TagAccess.DataSource = System.Enum.GetValues(typeof(TagAccessType));
            foreach (TagAccessType item in Enum.GetValues(typeof(TagAccessType)))
            {
                this.COMBOBOX_TagAccess.Items.Add(item);
            }
            COMBOBOX_TagAccess.Items.Remove(TagAccessType.QT_None);
            COMBOBOX_TagAccess.Items.Remove(TagAccessType.QT_Read);
            COMBOBOX_TagAccess.Items.Remove(TagAccessType.QT_Write);

            foreach (MemoryBank item in Enum.GetValues(typeof(MemoryBank)))
            {
                this.COMBOBOX_TagAccessMemoryBank.Items.Add(item);
            }
            this.COMBOBOX_TagAccessMemoryBank.Items.Remove(rfid.Constants.MemoryBank.UNKNOWN);
            this.COMBOBOX_TagAccessMemoryBank.SelectedIndex = 0;

            foreach (PasswordPermission item in Enum.GetValues(typeof(PasswordPermission)))
            {
                this.COMBOBOX_AccessPasswordPermissions.Items.Add(item);
                this.COMBOBOX_KillPasswordPermissions.Items.Add(item);
            }
            this.COMBOBOX_AccessPasswordPermissions.Items.Remove(PasswordPermission.UNKNOWN);
            this.COMBOBOX_KillPasswordPermissions.Items.Remove(PasswordPermission.UNKNOWN);
            this.COMBOBOX_AccessPasswordPermissions.SelectedIndex = 0;
            this.COMBOBOX_KillPasswordPermissions.SelectedIndex = 0;

            foreach (MemoryPermission item in Enum.GetValues(typeof(MemoryPermission)))
            {
                this.COMBOBOX_EPCBankPermissions.Items.Add(item);
                this.COMBOBOX_TIDBankPermissions.Items.Add(item);
                this.COMBOBOX_UserBankPermissions.Items.Add(item);
            }
            this.COMBOBOX_EPCBankPermissions.Items.Remove(MemoryPermission.UNKNOWN);
            this.COMBOBOX_TIDBankPermissions.Items.Remove(MemoryPermission.UNKNOWN);
            this.COMBOBOX_UserBankPermissions.Items.Remove(MemoryPermission.UNKNOWN);
            this.COMBOBOX_EPCBankPermissions.SelectedIndex = 0;
            this.COMBOBOX_TIDBankPermissions.SelectedIndex = 0;
            this.COMBOBOX_UserBankPermissions.SelectedIndex = 0;

            foreach (QTCtrlType item in Enum.GetValues(typeof(QTCtrlType)))
            {
                this.COMBOBOX_QTCtrlType.Items.Add(item);
            }
            this.COMBOBOX_QTCtrlType.Items.Remove(QTCtrlType.UNKNOWN);
            this.COMBOBOX_QTCtrlType.SelectedIndex = 0;

            foreach (QTPersistenceType item in Enum.GetValues(typeof(QTPersistenceType)))
            {
                this.COMBOBOX_QTPersistence.Items.Add(item);
            }
            this.COMBOBOX_QTPersistence.Items.Remove(QTPersistenceType.UNKNOWN);
            this.COMBOBOX_QTPersistence.SelectedIndex = 0;

            foreach (QTShortRangeType item in Enum.GetValues(typeof(QTShortRangeType)))
            {
                this.COMBOBOX_QTShortRange.Items.Add(item);
            }
            this.COMBOBOX_QTShortRange.Items.Remove(QTShortRangeType.UNKNOWN);
            this.COMBOBOX_QTShortRange.SelectedIndex = 0;

            foreach (QTMemMapType item in Enum.GetValues(typeof(QTMemMapType)))
            {
                this.COMBOBOX_QTMemMap.Items.Add(item);
            }
            this.COMBOBOX_QTMemMap.Items.Remove(QTMemMapType.UNKNOWN);
            this.COMBOBOX_QTMemMap.SelectedIndex = 0;
        }


        public FORM_TagAccess( LakeChabotReader reader, TagAccessData r_tagAccessData)
        {
            Init(reader);

            _tagAccessData = r_tagAccessData;

            if (!_tagAccessData.initialized)
            {
                _tagAccessData.accessPasswordPermissions = PasswordPermission.NO_CHANGE;
                _tagAccessData.killPasswordPermissions   = PasswordPermission.NO_CHANGE;
                _tagAccessData.epcMemoryBankPermissions  = MemoryPermission.NO_CHANGE;
                _tagAccessData.tidMemoryBankPermissions  = MemoryPermission.NO_CHANGE;
                _tagAccessData.userMemoryBankPermissions = MemoryPermission.NO_CHANGE;

                _tagAccessData.offset_text = "0";
                _tagAccessData.value1_text = "0";
                _tagAccessData.value2_text = "0";
                _tagAccessData.accessPassword_text = "0";
                _tagAccessData.killPassword_text = "0";

                _tagAccessData.count = 1;
                _tagAccessData.bank  = MemoryBank.EPC;

                _tagAccessData.strcTagFlag.PostMatchFlag     = 0;
                _tagAccessData.strcTagFlag.SelectOpsFlag     = 0;
                _tagAccessData.strcTagFlag.RetryCount        = 0;
                _tagAccessData.strcTagFlag.bErrorKeepRunning = false;

                _tagAccessData.initialized = true;
            }


            //Get Access Password in modeule's buffer.
            UInt32 Password = 0;
            if ( Result.OK == reader.API_l8K6CTagGetAccessPassword(ref Password) )
            {
                _tagAccessData.accessPassword_text = String.Format("{0:X}",Password);
                _tagAccessData.accessPassword      = Password;
            }

            this.COMBOBOX_TagAccess.SelectedIndex = (int)_tagAccessData.type;
            this.COMBOBOX_TagAccessMemoryBank.SelectedIndex = (int)_tagAccessData.bank;
            this.TEXTBOX_TagAccessOffset.Text = _tagAccessData.offset_text;

            this.TEXTBOX_TagAccessValue1.Text = _tagAccessData.value1_text;
            this.TEXTBOX_TagAccessValue2.Text = _tagAccessData.value2_text; 
            this.NUMERICUPDOWN_TagAccessCount.Value   = _tagAccessData.count;
            this.TEXTBOX_TagAccessAccessPassword.Text = _tagAccessData.accessPassword_text;
            this.TEXTBOX_TagAccessKillPassword.Text   = _tagAccessData.killPassword_text;
            this.COMBOBOX_AccessPasswordPermissions.SelectedIndex = (int)_tagAccessData.accessPasswordPermissions;
            this.COMBOBOX_KillPasswordPermissions.SelectedIndex   = (int)_tagAccessData.killPasswordPermissions;
            this.COMBOBOX_EPCBankPermissions.SelectedIndex  = (int)_tagAccessData.epcMemoryBankPermissions;
            this.COMBOBOX_TIDBankPermissions.SelectedIndex  = (int)_tagAccessData.tidMemoryBankPermissions;
            this.COMBOBOX_UserBankPermissions.SelectedIndex = (int)_tagAccessData.userMemoryBankPermissions;
            this.COMBOBOX_QTCtrlType.SelectedIndex    = (int)_tagAccessData.qtReadWrite;
            this.COMBOBOX_QTPersistence.SelectedIndex = (int)_tagAccessData.qtPersistence;
            this.COMBOBOX_QTShortRange.SelectedIndex  = (int)_tagAccessData.qtShortRange;
            this.COMBOBOX_QTMemMap.SelectedIndex     = (int)_tagAccessData.qtMemoryMap;
                           
                          
            if( _tagAccessData.strcTagFlag.SelectOpsFlag == 1)
                chkPerformSelectOps.Checked = true;
 
            if( _tagAccessData.strcTagFlag.PostMatchFlag == 1)
                chkPerformPostMatch.Checked = true;                    
        }


        public FORM_TagAccess(LakeChabotReader reader)
		{
            Init(reader);
        }

        private void ValidateHexInput(object sender, KeyEventArgs e)
        {
            ValidateHexInput(sender);
        }

        private void ValidateHexInput(object sender, MouseEventArgs e)
        {
            ValidateHexInput(sender);
        }

        private void ValidateHexInput(object sender)
        {
            if (typeof(TextBox) == sender.GetType())
            {
                TextBox textBox = (TextBox)sender;
                string originalText = textBox.Text;
                int originalSelection = textBox.SelectionStart;
                Regex regEx = new Regex("[^0-9A-Fa-f]");
                string newString = regEx.Replace(textBox.Text, "").ToUpper();
                textBox.Text = newString;
                if (newString != originalText)
                {
                    textBox.SelectionStart = originalSelection;
                    textBox.ScrollToCaret();
                }
            }
            else if (typeof(ComboBox) == sender.GetType())
            {
                ComboBox comboBox = (ComboBox)sender;
                string originalText = comboBox.Text;
                int originalSelection = comboBox.SelectionStart;
                Regex regEx = new Regex("[^0-9A-Fa-f]");
                string newString = regEx.Replace(comboBox.Text, "").ToUpper();
                comboBox.Text = newString;
                if (newString != originalText)
                {
                    comboBox.SelectionStart = originalSelection;
                }
            }
        }

        private bool ValidateHex_uint(string input, string name, out uint value)
        {
            if (!uint.TryParse(input, NumberStyles.AllowHexSpecifier, null, out value))
            {
                MessageBox.Show(string.Format("{0} [{1}]", name, input),
                                "Tag Access Input Invalid",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }

        private bool ValidateHex_ushort(string input, string name, out ushort value)
        {
            if (!ushort.TryParse(input, NumberStyles.AllowHexSpecifier, null, out value))
            {
                MessageBox.Show(string.Format("{0} [{1}]", name, input),
                                "Tag Access Input Invalid",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

	    private void okButton_Click(object sender, EventArgs e)
		{
            bool valid = true;

            _tagAccessData.type = (TagAccessType)COMBOBOX_TagAccess.SelectedIndex;

            _tagAccessData.offset_text = this.TEXTBOX_TagAccessOffset.Text;
            _tagAccessData.value1_text = this.TEXTBOX_TagAccessValue1.Text;
            _tagAccessData.value2_text = this.TEXTBOX_TagAccessValue2.Text;
            _tagAccessData.accessPassword_text = this.TEXTBOX_TagAccessAccessPassword.Text;
            _tagAccessData.killPassword_text   = this.TEXTBOX_TagAccessKillPassword.Text;

            //assign ReadWords, TotalReadWords??
            _tagAccessReads.ReadWords = Int32.Parse(COMBOBOX_TagAccessReadWords.Text);
            _tagAccessReads.TotalReadWords = Int32.Parse(TEXTBOX_TagAccessTotalReadWords.Text);
            //TagAccessReadSet.ReadWords = Convert.ToInt32(COMBOBOX_TagAccessReadWords);
            //TagAccessReadSet.TotalReadWords = Convert.ToInt32(TEXTBOX_TagAccessTotalReadWords);

            //clark 2011.4.22  Let every access type has different flag.
            _tagAccessData.strcTagFlag.SelectOpsFlag = (byte)(chkPerformSelectOps.Checked ? 1 : 0);
            _tagAccessData.strcTagFlag.PostMatchFlag = (byte)(chkPerformPostMatch.Checked ? 1 : 0);

            ////Clark 2011.08.18 Set Access Password in modeule's buffer.
            valid &= ValidateHex_uint(TEXTBOX_TagAccessAccessPassword.Text, "Set Password", out _tagAccessData.accessPassword);
          
            if 
            ( 
                (valid != true ) 
                ||                 
                Result.OK != _reader.API_l8K6CSetTagAccessPassword(_tagAccessData.accessPassword) 
            )
            { 
                MessageBox.Show( "Set Password fail",
                                 "Password",
                                 MessageBoxButtons.OK, 
                                 MessageBoxIcon.Exclamation );
                
                return;
            }



            switch (_tagAccessData.type)
            {
                case TagAccessType.Read:
                case TagAccessType.QT_Read:
                    _tagAccessData.bank = (MemoryBank)COMBOBOX_TagAccessMemoryBank.SelectedIndex;
                    valid &= ValidateHex_ushort(TEXTBOX_TagAccessOffset.Text, "Offset", out _tagAccessData.offset);
                    _tagAccessData.count = (byte)NUMERICUPDOWN_TagAccessCount.Value;
                    valid &= ValidateHex_uint(TEXTBOX_TagAccessAccessPassword.Text, "Access Password", out _tagAccessData.accessPassword);
                    break;

                //AddLargeRead command
                case TagAccessType.LargeRead:
                    _tagAccessData.bank = (MemoryBank)COMBOBOX_TagAccessMemoryBank.SelectedIndex;
                    valid &= ValidateHex_ushort(TEXTBOX_TagAccessOffset.Text, "Offset", out _tagAccessData.offset);//Offset為零
                    _tagAccessData.count = (byte)NUMERICUPDOWN_TagAccessCount.Value;
                    valid &= ValidateHex_uint(TEXTBOX_TagAccessAccessPassword.Text, "Access Password", out _tagAccessData.accessPassword);
                    break;

                case TagAccessType.Write:
                case TagAccessType.QT_Write:
                    _tagAccessData.bank = (MemoryBank)COMBOBOX_TagAccessMemoryBank.SelectedIndex;
                    valid &= ValidateHex_ushort(TEXTBOX_TagAccessOffset.Text, "Offset", out _tagAccessData.offset);
                    valid &= ValidateHex_ushort(TEXTBOX_TagAccessValue1.Text, "Value", out _tagAccessData.value1);
                    _tagAccessData.count = (byte)NUMERICUPDOWN_TagAccessCount.Value;
                    if (_tagAccessData.count == 2)
                    {
                        valid &= ValidateHex_ushort(TEXTBOX_TagAccessValue2.Text, "Value 2", out _tagAccessData.value2);
                    }
                    valid &= ValidateHex_uint(TEXTBOX_TagAccessAccessPassword.Text, "Access Password", out _tagAccessData.accessPassword);
                    break;

                case TagAccessType.BlockWrite:
                    _tagAccessData.bank = (MemoryBank)COMBOBOX_TagAccessMemoryBank.SelectedIndex;
                    valid &= ValidateHex_ushort(TEXTBOX_TagAccessOffset.Text, "Offset", out _tagAccessData.offset);
                    valid &= ValidateHex_ushort(TEXTBOX_TagAccessValue1.Text, "Value 1", out _tagAccessData.value1);
                    _tagAccessData.count = (byte)NUMERICUPDOWN_TagAccessCount.Value;
                    if (_tagAccessData.count == 2)
                    {
                        valid &= ValidateHex_ushort(TEXTBOX_TagAccessValue2.Text, "Value 2", out _tagAccessData.value2);
                    }
                    valid &= ValidateHex_uint(TEXTBOX_TagAccessAccessPassword.Text, "Access Password", out _tagAccessData.accessPassword);
                    break;

                case TagAccessType.BlockErase:
                    _tagAccessData.bank = (MemoryBank)COMBOBOX_TagAccessMemoryBank.SelectedIndex;
                    valid &= ValidateHex_ushort(TEXTBOX_TagAccessOffset.Text, "Offset", out _tagAccessData.offset);
                    _tagAccessData.count = (byte)NUMERICUPDOWN_TagAccessCount.Value;
                    valid &= ValidateHex_uint(TEXTBOX_TagAccessAccessPassword.Text, "Access Password", out _tagAccessData.accessPassword);
                    break;

                case TagAccessType.Lock:
                    _tagAccessData.killPasswordPermissions   = (PasswordPermission)COMBOBOX_KillPasswordPermissions.SelectedIndex;
                    _tagAccessData.accessPasswordPermissions = (PasswordPermission)COMBOBOX_AccessPasswordPermissions.SelectedIndex;
                    _tagAccessData.epcMemoryBankPermissions  = (MemoryPermission)COMBOBOX_EPCBankPermissions.SelectedIndex;
                    _tagAccessData.tidMemoryBankPermissions  = (MemoryPermission)COMBOBOX_TIDBankPermissions.SelectedIndex;
                    _tagAccessData.userMemoryBankPermissions = (MemoryPermission)COMBOBOX_UserBankPermissions.SelectedIndex;
                    valid &= ValidateHex_uint(TEXTBOX_TagAccessAccessPassword.Text, "Access Password", out _tagAccessData.accessPassword);
                    break;

                case TagAccessType.Kill:
                    valid &= ValidateHex_uint(TEXTBOX_TagAccessKillPassword.Text, "Kill Password", out _tagAccessData.killPassword);
                    valid &= ValidateHex_uint(TEXTBOX_TagAccessAccessPassword.Text, "Access Password", out _tagAccessData.accessPassword);
                    break;

                case TagAccessType.QT_None:
                    break;

                default:
                    System.Diagnostics.Debug.Assert(false, "Tag Acces Type"); 
                    break;
            }


            switch (_tagAccessData.type)
            {
                case TagAccessType.QT_Read:
                case TagAccessType.QT_Write:
                case TagAccessType.QT_None:
                    _tagAccessData.qtReadWrite   = (QTCtrlType)COMBOBOX_QTCtrlType.SelectedIndex;
                    _tagAccessData.qtPersistence = (QTPersistenceType)COMBOBOX_QTPersistence.SelectedIndex;
                    _tagAccessData.qtShortRange  = (QTShortRangeType)COMBOBOX_QTShortRange.SelectedIndex;
                    _tagAccessData.qtMemoryMap   = (QTMemMapType)COMBOBOX_QTMemMap.SelectedIndex;
                    break;
                default:
                    break;
            }


            if (valid)
            {
                DialogResult = DialogResult.OK;
            }

            return;
		}

        string offsetText1, offsetText2;//∴
        bool flag=false;
        private void COMBOBOX_TagAccess_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.COMBOBOX_TagAccessMemoryBank.Items.Clear();
            foreach (MemoryBank item in Enum.GetValues(typeof(MemoryBank)))
            {
                this.COMBOBOX_TagAccessMemoryBank.Items.Add(item);
            }
            this.COMBOBOX_TagAccessMemoryBank.Items.Remove(rfid.Constants.MemoryBank.UNKNOWN);
            this.COMBOBOX_TagAccessMemoryBank.SelectedIndex = 0;

            LABEL_TagAccessMemoryBank.Visible = COMBOBOX_TagAccessMemoryBank.Visible = false;
            LABEL_TagAccessOffset.Visible     = TEXTBOX_TagAccessOffset.Visible      = false;
            LABEL_TagAccessValue1.Visible     = TEXTBOX_TagAccessValue1.Visible      = false;
            LABEL_TagAccessValue2.Visible     = TEXTBOX_TagAccessValue2.Visible      = false;

            COMBOBOX_TagAccessReadWords.SelectedIndex = 0;
            //TEXTBOX_TagAccessOffset.Text = "0";//??Offset
            LABEL_TagAccessReadWords.Visible = COMBOBOX_TagAccessReadWords.Visible = false;
            LABEL_TagAccessTotalReadWords.Visible = TEXTBOX_TagAccessTotalReadWords.Visible = false;   

            LABEL_TagAccessCount.Visible      = NUMERICUPDOWN_TagAccessCount.Visible = false;
            NUMERICUPDOWN_TagAccessCount.Minimum = 0;
            NUMERICUPDOWN_TagAccessCount.Maximum = 255;

            LABEL_TagAccessAccessPassword.Visible = TEXTBOX_TagAccessAccessPassword.Visible = false;
            LABEL_TagAccessKillPassword.Visible   = TEXTBOX_TagAccessKillPassword.Visible   = false;

            GROUPBOX_TagAccessPermissions.Visible = false;
            GROUPBOX_TagAccessQTControl.Visible   = false;
            COMBOBOX_TagAccessMemoryBank.Enabled = true;
            TEXTBOX_TagAccessOffset.Enabled = true;//∴
            if (flag == true)//∴
            {
                TEXTBOX_TagAccessOffset.Text = offsetText2;
                flag = false;
            }

            if ((TagAccessType)COMBOBOX_TagAccess.SelectedIndex == TagAccessType.QT_Read ||
               (TagAccessType)COMBOBOX_TagAccess.SelectedIndex == TagAccessType.QT_Write)
            //if ((TagAccessType)COMBOBOX_TagAccess.SelectedIndex == TagAccessType.QT_Read ||
            //  (TagAccessType)COMBOBOX_TagAccess.SelectedIndex == TagAccessType.QT_Write)
            {
                GROUPBOX_TagAccessQTControl.Visible = true;
            }

            switch ((TagAccessType)COMBOBOX_TagAccess.SelectedIndex)
            //switch ((TagAccessType)COMBOBOX_TagAccess.SelectedIndex)
            {
                case TagAccessType.QT_Read:
                case TagAccessType.Read:
                    LABEL_TagAccessMemoryBank.Visible     = COMBOBOX_TagAccessMemoryBank.Visible    = true;
                    LABEL_TagAccessOffset.Visible         = TEXTBOX_TagAccessOffset.Visible         = true;
                    //TEXTBOX_TagAccessOffset.Enabled = true;//
                    LABEL_TagAccessCount.Visible          = NUMERICUPDOWN_TagAccessCount.Visible    = true;
                    LABEL_TagAccessAccessPassword.Visible = TEXTBOX_TagAccessAccessPassword.Visible = true;
                    NUMERICUPDOWN_TagAccessCount.Value    = 1;
                    NUMERICUPDOWN_TagAccessCount.Minimum  = 1;
                    NUMERICUPDOWN_TagAccessCount.Maximum  = 255;
                    break;

                //Add LargeRead command
                case TagAccessType.LargeRead:
                    this.COMBOBOX_TagAccessMemoryBank.Items.Clear();
                    foreach (MemoryBank item in Enum.GetValues(typeof(MemoryBank)))
                    {
                        this.COMBOBOX_TagAccessMemoryBank.Items.Add(item);
                    }
                    this.COMBOBOX_TagAccessMemoryBank.Items.Remove(rfid.Constants.MemoryBank.UNKNOWN);
                    this.COMBOBOX_TagAccessMemoryBank.SelectedIndex = 3;
                    COMBOBOX_TagAccessMemoryBank.Enabled = false;
                    LABEL_TagAccessMemoryBank.Visible = COMBOBOX_TagAccessMemoryBank.Visible = true;
                    LABEL_TagAccessOffset.Visible = TEXTBOX_TagAccessOffset.Visible = true;
                    offsetText2 = TEXTBOX_TagAccessOffset.Text;
                    TEXTBOX_TagAccessOffset.Text = "0";//∴
                    flag = true;
                    TEXTBOX_TagAccessOffset.Enabled = false;   //固定Offset欄位，鎖定為0
                    LABEL_TagAccessCount.Visible = NUMERICUPDOWN_TagAccessCount.Visible = false;    //取消count欄
                    LABEL_TagAccessReadWords.Visible = COMBOBOX_TagAccessReadWords.Visible = true;
                    LABEL_TagAccessTotalReadWords.Visible = TEXTBOX_TagAccessTotalReadWords.Visible = true;
                    TEXTBOX_TagAccessTotalReadWords.Enabled = false;
                    LABEL_TagAccessAccessPassword.Visible = TEXTBOX_TagAccessAccessPassword.Visible = true;
                    NUMERICUPDOWN_TagAccessCount.Value = 1;
                    break;

                case TagAccessType.QT_Write:
                case TagAccessType.Write:
                case TagAccessType.BlockWrite:
                    LABEL_TagAccessMemoryBank.Visible     = COMBOBOX_TagAccessMemoryBank.Visible = true;
                    LABEL_TagAccessOffset.Visible         = TEXTBOX_TagAccessOffset.Visible      = true;
                    LABEL_TagAccessValue1.Visible         = TEXTBOX_TagAccessValue1.Visible      = true;
                    LABEL_TagAccessValue2.Visible         = TEXTBOX_TagAccessValue2.Visible      = true;
                    LABEL_TagAccessCount.Visible          = NUMERICUPDOWN_TagAccessCount.Visible = true;
                    TEXTBOX_TagAccessValue2.Enabled       = false;
                    NUMERICUPDOWN_TagAccessCount.Value    = 1;
                    NUMERICUPDOWN_TagAccessCount.Minimum  = 1;
                    NUMERICUPDOWN_TagAccessCount.Maximum  = 2;
                    LABEL_TagAccessAccessPassword.Visible = TEXTBOX_TagAccessAccessPassword.Visible = true;
                    break;

                case TagAccessType.BlockErase:
                    LABEL_TagAccessMemoryBank.Visible     = COMBOBOX_TagAccessMemoryBank.Visible    = true;
                    LABEL_TagAccessOffset.Visible         = TEXTBOX_TagAccessOffset.Visible         = true;
                    //TEXTBOX_TagAccessOffset.Enabled = true;//
                    LABEL_TagAccessCount.Visible          = NUMERICUPDOWN_TagAccessCount.Visible    = true;
                    LABEL_TagAccessAccessPassword.Visible = TEXTBOX_TagAccessAccessPassword.Visible = true;
                    NUMERICUPDOWN_TagAccessCount.Value    = 1;
                    NUMERICUPDOWN_TagAccessCount.Minimum  = 1;
                    break;

                case TagAccessType.Lock:
                    GROUPBOX_TagAccessPermissions.Visible = true;
                    LABEL_TagAccessAccessPassword.Visible = TEXTBOX_TagAccessAccessPassword.Visible = true;
                    break;

                case TagAccessType.Kill:
                    LABEL_TagAccessAccessPassword.Visible = TEXTBOX_TagAccessAccessPassword.Visible = true;
                    LABEL_TagAccessKillPassword.Visible   = TEXTBOX_TagAccessKillPassword.Visible   = true;
                    break;

                case TagAccessType.QT_None:
                    GROUPBOX_TagAccessQTControl.Visible = true;
                    break;

                default:
                    System.Diagnostics.Debug.Assert(false, "Tag Acces Type");
                    break;
            }
        }

        private void COMBOBOX_QTCtrlType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LABEL_QTAccess.Visible      = COMBOBOX_QTCtrlType.Visible    = false;
            LABEL_QTPersistence.Visible = COMBOBOX_QTPersistence.Visible = false;
            LABEL_QTRange.Visible       = COMBOBOX_QTShortRange.Visible  = false;
            LABEL_QTMemory.Visible      = COMBOBOX_QTMemMap.Visible      = false;

            switch ((QTCtrlType)COMBOBOX_QTCtrlType.SelectedIndex)
            {
                case QTCtrlType.READ:
                    LABEL_QTAccess.Visible = COMBOBOX_QTCtrlType.Visible = true;
                    break;
                case QTCtrlType.WRITE:
                    LABEL_QTAccess.Visible      = COMBOBOX_QTCtrlType.Visible    = true;
                    LABEL_QTPersistence.Visible = COMBOBOX_QTPersistence.Visible = true;
                    LABEL_QTRange.Visible       = COMBOBOX_QTShortRange.Visible  = true;
                    LABEL_QTMemory.Visible      = COMBOBOX_QTMemMap.Visible      = true;
                    break;
                default:
                    break;
            }

        }

        private void NUMERICUPDOWN_TagAccessCount_ValueChanged(object sender, EventArgs e)
        {
            if (NUMERICUPDOWN_TagAccessCount.Value == 2)
            {
                TEXTBOX_TagAccessValue2.Enabled = true;
            }
            else
            {
                TEXTBOX_TagAccessValue2.Enabled = false;
            }
        }
    }
}