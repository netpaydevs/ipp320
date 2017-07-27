using Com.Netpay.Pinpad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Ipp320WindowsConnector
{
    public partial class FormNew6210 : Form
    {

        //dev 17
        private static readonly string url1 = "https://200.57.87.243/acquirertst";
        private static readonly string url2 = "https://200.57.87.243/acquirertst";
        private static readonly string storeId = "8889";
        private static readonly string password = "adm0n2";
        private static readonly string user = "POS";

        //20
        //private static readonly string url1 = "https://cert.netpay.com.mx/acqqaj";
        //private static readonly string url2 = "https://cert.netpay.com.mx/acqqaj";
        //private static readonly string storeId = "9536";
        //private static readonly string password = "adm0n2";
        //private static readonly string user = "9090";

        //preprd 35
        //private static readonly string url1 = "https://suitedrp.netpay.com.mx/acquirertstj";
        //private static readonly string url2 = "https://suitedrp.netpay.com.mx/acquirertstj";
        //private static readonly string storeId = "453175";
        //private static readonly string password = "615303";

        //prd
        //private static readonly string url1 = "https://suite.netpay.com.mx/acquirerprdj";
        //private static readonly string url2 = "https://suite.netpay.com.mx/acquirerprdj";
        //private static readonly string storeId = "100940";
        //private static readonly string user = "POS";
        //private static readonly string password = "198036";

        private static readonly string portName = "COM7";
        private static readonly string terminalId = "00010001";

        private static readonly string timeOut = "30000";
        private static Pinpad1000SEManager pinpadManager;
        private static string amount;
        private static string transactionType;
        private static string orderId;
        private static string promotion;

        private static readonly string FILENAME = "./Log_New6210.txt";
        private static FileStream ostrm;
        private static StreamWriter writer;
        private static TextWriter oldOut;

        public FormNew6210()
        {
            InitializeComponent();
            this.FormClosing += FormNew6210_FormClosing;

            Dictionary<string, string> transactionTypes = new Dictionary<string, string>();
            transactionTypes.Add("Auth", "Auth");
            transactionTypes.Add("Refund", "Refund");
            transactionTypes.Add("Credit", "Credit");
            transactionTypes.Add("Reverse", "Reverse");
            transactionTypes.Add("FindTransaction", "FindTransaction");
            comboBox1.DataSource = new BindingSource(transactionTypes, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            Debug.WriteLine("Entering application.");
        }

        private void FormNew6210_Load(object sender, EventArgs e)
        {
            this.Text = "New6210Demo";
            pinpadManager = new Pinpad1000SEManager(portName, url1, url2, timeOut);
                               
            oldOut = Console.Out;
            try
            {
                ostrm = new FileStream(FILENAME, FileMode.Append, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot open Redirect.txt for writing");
                Console.WriteLine(ex.Message);
                return;
            }
            Console.SetOut(writer);

            pinpadManager.useTLS1_2 = true; 
            Console.WriteLine("Use TLS1.2 " + pinpadManager.useTLS1_2);

        }

        private void FormNew6210_FormClosing(object sender, FormClosingEventArgs e)
        {            
            pinpadManager = null;
            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show("Hay un proceso de lectura en ejecución");
            }
            else
            {
                string text = this.textBox1.Text;
                if (text == null || text.Trim().Length == 0)
                {
                    MessageBox.Show("Agregue un texto");
                    return;
                }
                pinpadManager.Display(text);
            }
        }
                        
        private void DisplayError()
        {
            this.textBox2.Clear();
            this.textBox2.Text = "RESPONSE_CODE " + pinpadManager.PtResponseCode + System.Environment.NewLine;
            this.textBox2.Text += "RESPONSE_MSG " + pinpadManager.PtResponseMsg + System.Environment.NewLine;
            this.textBox2.Text += "RESPONSE_TXT " + pinpadManager.PtResponseText;
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Console.WriteLine("amount " + amount);
            pinpadManager.ProcessTransaction(transactionType, storeId, user, password, terminalId, promotion, amount, "", orderId, null, "P", "", "", "", "", "", "", "0");            
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {            
            if (pinpadManager.PtResponseCode.Equals("00"))
            {
                DisplayResponse();
            }
            else
            {                
                DisplayError();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show("Hay un proceso de lectura en ejecución");
            }
            else
            {
                pinpadManager.InitKeys("LoadInitKey", storeId, terminalId, user, password, url1, url2, timeOut);
                DisplayError();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show("Hay un proceso de lectura en ejecución");
            }
            else
            {
                this.textBox2.Clear();
                string strAmount = this.textBox3.Text;
                transactionType = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Value;
                orderId = this.textBox4.Text;                
                if (strAmount == null || strAmount.Trim().Length == 0)
                {
                    MessageBox.Show("Agregue un monto");
                    return;
                }
                else
                {
                    double d_amount = Double.Parse(strAmount);
                    if (d_amount <= 0)
                    {
                        MessageBox.Show("Monto incorrecto");
                        return;
                    }                    
                    amount = this.textBox3.Text;                    
                }

                string strPromotion = this.textBox5.Text;
                if (string.IsNullOrEmpty(strPromotion))
                {
                    strPromotion = "000000";					
                }

                if (strPromotion.Length != 6)
                {
                    MessageBox.Show("Promocion otorgada invalida, longitud incorrecta.");                    
                    return;
                }

                string larrefed = strPromotion.Substring(0, 2);
                string months = strPromotion.Substring(2, 2);
                string nalp = strPromotion.Substring(4, 2);

                if (larrefed != "00" && strPromotion != "000000")
                {
                    MessageBox.Show("Diferido de promocion otorgada invalido.");                    
                    return;
                }
                if (months == "00" && strPromotion != "000000")
                {
                    MessageBox.Show("Plan de meses invalido.");
                    return;
                }
                if (nalp != "03" && strPromotion != "000000")
                {
                    MessageBox.Show("Plan de promocion otorgada invalido.");                    
                    return;
                }
                promotion = strPromotion;

                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 8)
            {
                e.Handled = false;
                return;
            }

            bool IsDec = false;
            int nroDec = 0;

            for (int i = 0; i < textBox1.Text.Length; i++)
            {
                if (textBox1.Text[i] == '.')
                {
                    IsDec = true;
                }
                if (IsDec && nroDec++ >= 2)
                {
                    e.Handled = true;
                    return;
                }
            }

            if (e.KeyChar >= 48 && e.KeyChar <= 57)
            {
                e.Handled = false;
            }
            else if (e.KeyChar == 46)
            {
                e.Handled = (IsDec) ? true : false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show("Hay un proceso de lectura en ejecución");
            }
            else
            {
                this.textBox2.Clear();
                string strAmount = this.textBox3.Text;
                if (strAmount == null || strAmount.Trim().Length == 0)
                {
                    MessageBox.Show("Agregue un monto");
                    return;
                }
                else
                {
                    double d_amount = Double.Parse(strAmount);
                    if (d_amount <= 0)
                    {
                        MessageBox.Show("Monto incorrecto");
                        return;
                    }                    
                    amount = this.textBox3.Text;
                }
                orderId = this.textBox4.Text;
                if (orderId == null || orderId.Trim().Length == 0)
                {
                    MessageBox.Show("Agregue un id de orden");
                    return;
                }
                else
                {
                    transactionType = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Value;
                    pinpadManager.ProcessTransaction(transactionType, storeId, user, password, terminalId, null, amount, null, orderId, null, "P", null, null, null, null, null, null, null);
                    DisplayResponse();
                }
            }
        }

        private void DisplayResponse()
        {
            this.textBox2.Text = "ORDER_ID " + pinpadManager.PtOrderId;
            this.textBox2.Text += System.Environment.NewLine + "TRANSACTION_ID: " + pinpadManager.PtTransactionId;
            this.textBox2.Text += System.Environment.NewLine + "AID: " + pinpadManager.PtAID;
            this.textBox2.Text += System.Environment.NewLine + "ARQC: " + pinpadManager.PtARQC;
            this.textBox2.Text += System.Environment.NewLine + "TSI: " + pinpadManager.PtTSI;
            this.textBox2.Text += System.Environment.NewLine + "TVR: " + pinpadManager.PtTVR;
            this.textBox2.Text += System.Environment.NewLine + "APN: " + pinpadManager.PtAPN;
            this.textBox2.Text += System.Environment.NewLine + "AUTH_CODE: " + pinpadManager.PtAuthCode;
            this.textBox2.Text += System.Environment.NewLine + "BANK_NAME: " + pinpadManager.PtBankName;
            this.textBox2.Text += System.Environment.NewLine + "CARD_TYPE: " + pinpadManager.PtCardType;
            this.textBox2.Text += System.Environment.NewLine + "CARD_TYPE_NAME: " + pinpadManager.PtCardTypeName;
            this.textBox2.Text += System.Environment.NewLine + "CUSTOMER_NAME: " + pinpadManager.PtCustomerName;
            this.textBox2.Text += System.Environment.NewLine + "CARD_NUMBER: " + pinpadManager.PtCardNumber;
            this.textBox2.Text += System.Environment.NewLine + "CVM: " + pinpadManager.PtCVM;
            this.textBox2.Text += System.Environment.NewLine + "MERCHANT_ID: " + pinpadManager.PtMerchantId;
            this.textBox2.Text += System.Environment.NewLine + "RESPONSE_CODE: " + pinpadManager.PtResponseCode;
            this.textBox2.Text += System.Environment.NewLine + "RESPONSE_MSG: " + pinpadManager.PtResponseMsg;
            this.textBox2.Text += System.Environment.NewLine + "RESPONSE_TEXT: " + pinpadManager.PtResponseText;
            this.textBox2.Text += System.Environment.NewLine + "POS_ENTRY_MODE: " + pinpadManager.PtPOSEntryMode;
            this.textBox2.Text += System.Environment.NewLine + "CARD_TOKEN: " + pinpadManager.PtCardToken;
        }

    }

}
