using Com.Netpay.Pinpad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace Ipp320WindowsConnector
{
    public partial class Form1 : Form
    {
        
        //dev 17
        private static readonly string url1 = "https://200.57.87.243/acquirertst";
        private static readonly string url2 = "https://200.57.87.243/acquirertst";
        private static readonly string storeId = "8889";
        private static readonly string password = "adm0n2";

        //20
        //private static readonly string url1 = "https://200.57.87.242:8866";
        //private static readonly string url2 = "https://200.57.87.242:8866";
        //private static readonly string storeId = "1100120";

        //preprd 35
        //private static readonly string url1 = "https://suitedrp.netpay.com.mx/acquirertstj";
        //private static readonly string url2 = "https://suitedrp.netpay.com.mx/acquirertstj";
        //private static readonly string storeId = "453175";
        //private static readonly string password = "615303";

        //prd
        //private static readonly string url4 = "https://suite.netpay.com.mx/acquirerprdj";
        //private static readonly string storeId = "100940";
        //private static readonly string password = "198036";

        private static readonly string user = "POS";        
        private static readonly string timeOut = "30000";
        private static readonly string portName = "COM6";
        private static readonly string terminalId = "00010001";
        private static Ipp320Manager ipp320Manager;        
        private static string pan;
        private static bool isPartial = false;
        private static bool isReadCustomCard = false;
        private static bool isPartialFinished = true;
        private static string amount;
        private static string transactionType;
        private static string orderId;
        private static readonly double MAX_VALUE = 21473999.99;

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;

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

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Ipp320Demo";
            ipp320Manager = new Ipp320Manager(portName, url1, url2, timeOut, true);
            //ipp320Manager = new Ipp320Manager(portName, url1, url2, timeOut, false);
            //ipp320Manager = new Ipp320Manager(portName, urlPrePRD, urlPrePRD, timeOut, true);
            ipp320Manager.StartPCLService();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ipp320Manager.StopPCLService();
            ipp320Manager = null;
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
                ipp320Manager.Display(text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show("Hay un proceso de lectura en ejecución");
            }
            else
            {
                this.textBox2.Clear();
                ipp320Manager.ProcessManagement("GetVersion", "", "", "", null);
                if (ipp320Manager.PtResponseCode.Equals("00"))
                {
                    this.textBox2.Text = "Versión " + ipp320Manager.PtVersion;
                }
                else
                {
                    DisplayError();
                }
            }            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show("Hay un proceso de lectura en ejecución");
            }
            else
            {
                this.textBox2.Clear();
                isReadCustomCard = true;
                ipp320Manager.timeOutReadCard = 60; // segundos
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (backgroundWorker1.IsBusy)
            {
                MessageBox.Show("Hay un proceso de lectura en ejecución");
            }
            else
            {
                string strAmount = this.textBox3.Text;
                if(!string.IsNullOrEmpty(strAmount))
                {
                    double d_amount = Double.Parse(strAmount);
                    if (d_amount <= 0)
                    {
                        MessageBox.Show("Monto incorrecto");
                        return;
                    }
                    else if (d_amount > MAX_VALUE)
                    {
                        MessageBox.Show("Monto máximo no permitido");
                        return;
                    }
                    amount = this.textBox3.Text;
                }

                this.textBox2.Clear();
                isPartial = true;
                isPartialFinished = false;          
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void DisplayError()
        {
            this.textBox2.Clear();
            this.textBox2.Text = "RESPONSE_CODE " + ipp320Manager.PtResponseCode + System.Environment.NewLine;
            this.textBox2.Text += "RESPONSE_MSG " + ipp320Manager.PtResponseMsg;
        }

        private void button5_Click(object sender, EventArgs e)
        {            
            if (backgroundWorker1.IsBusy)
            {
                backgroundWorker1.CancelAsync();
                ipp320Manager.CancelTransaction();
                isPartial = false;
                isReadCustomCard = false;
            }
            else
            {
                if (isPartialFinished)
                {
                    ipp320Manager.VirtualCancelTransaction();
                }
                else
                {
                    MessageBox.Show("No hay ningún proceso con cancelación en ejecución");
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if(isPartial)
            {
                if (!string.IsNullOrEmpty(amount))
                {
                    ipp320Manager.PartialTransaction(out pan, amount);
                }
                else
                {
                    ipp320Manager.PartialTransaction(out pan);
                }
                
            }
            else if (isReadCustomCard)
            {
                ipp320Manager.ReadCustomCard();
            }
            else
            {
                ipp320Manager.ProcessTransaction(transactionType, storeId, user, password, terminalId, "000603", amount, "DSVRG", orderId, null, "P", "comentario", "", "", "", "", "", "0");
            }            
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {            
            if (isPartial && ipp320Manager.PtResponseCode.Equals("00"))
            {
                this.textBox2.Text = "PAN " + pan;
                isPartial = false;
                isPartialFinished = true;
            }
            else if (isReadCustomCard && ipp320Manager.PtResponseCode.Equals("00"))
            {
                this.textBox2.Text = "Track 1 " + ipp320Manager.PtTrack1 + System.Environment.NewLine;
                this.textBox2.Text += "Track 2 " + ipp320Manager.PtTrack2;
                isReadCustomCard = false;
            }
            else if( (!isPartial || !isReadCustomCard ) && ipp320Manager.PtResponseCode.Equals("00"))
            {
                DisplayResponse();
            }            
            else
            {
                isPartial = false;
                isReadCustomCard = false;              
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
                ipp320Manager.ProcessManagement("LoadInitKey", storeId, terminalId, user, password);
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
                isPartial = false;                
                string strAmount = this.textBox3.Text;
                transactionType = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Value;
                orderId = this.textBox4.Text;                
                ipp320Manager.timeOutReadCard = 480; // en segundos
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
                    else if(d_amount > MAX_VALUE)
                    {
                        MessageBox.Show("Monto máximo no permitido");
                        return;
                    }
                    amount = this.textBox3.Text;
                    backgroundWorker1.RunWorkerAsync();
                }
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
                    else if (d_amount > MAX_VALUE)
                    {
                        MessageBox.Show("Monto máximo no permitido");
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
                    ipp320Manager.ProcessTransaction(transactionType, storeId, user, password, terminalId, null, amount, null, orderId, null, "P", null, null, null, null, null, null, null);
                    DisplayResponse();
                }
            }
        }

        private void DisplayResponse()
        {
            this.textBox2.Text = "ORDER_ID " + ipp320Manager.PtOrderId;
            this.textBox2.Text += System.Environment.NewLine + "TRANSACTION_ID: " + ipp320Manager.PtTransactionId;
            this.textBox2.Text += System.Environment.NewLine + "AID: " + ipp320Manager.PtAID;
            this.textBox2.Text += System.Environment.NewLine + "ARQC: " + ipp320Manager.PtARQC;
            this.textBox2.Text += System.Environment.NewLine + "TSI: " + ipp320Manager.PtTSI;
            this.textBox2.Text += System.Environment.NewLine + "TVR: " + ipp320Manager.PtTVR;
            this.textBox2.Text += System.Environment.NewLine + "APN: " + ipp320Manager.PtAPN;
            this.textBox2.Text += System.Environment.NewLine + "AUTH_CODE: " + ipp320Manager.PtAuthCode;
            this.textBox2.Text += System.Environment.NewLine + "BANK_NAME: " + ipp320Manager.PtBankName;
            this.textBox2.Text += System.Environment.NewLine + "CARD_TYPE: " + ipp320Manager.PtCardType;
            this.textBox2.Text += System.Environment.NewLine + "CARD_TYPE_NAME: " + ipp320Manager.PtCardTypeName;
            this.textBox2.Text += System.Environment.NewLine + "CUSTOMER_NAME: " + ipp320Manager.PtCustomerName;
            this.textBox2.Text += System.Environment.NewLine + "CARD_NUMBER: " + ipp320Manager.PtCardNumber;
            this.textBox2.Text += System.Environment.NewLine + "CVM: " + ipp320Manager.PtCVM;
            this.textBox2.Text += System.Environment.NewLine + "MERCHANT_ID: " + ipp320Manager.PtMerchantId;
            this.textBox2.Text += System.Environment.NewLine + "RESPONSE_CODE: " + ipp320Manager.PtResponseCode;
            this.textBox2.Text += System.Environment.NewLine + "RESPONSE_MSG: " + ipp320Manager.PtResponseMsg;
            this.textBox2.Text += System.Environment.NewLine + "RESPONSE_TEXT: " + ipp320Manager.PtResponseText;
            this.textBox2.Text += System.Environment.NewLine + "POS_ENTRY_MODE: " + ipp320Manager.PtPOSEntryMode;
            this.textBox2.Text += System.Environment.NewLine + "CARD_TOKEN: " + ipp320Manager.PtCardToken;
        }
       
    }
}
