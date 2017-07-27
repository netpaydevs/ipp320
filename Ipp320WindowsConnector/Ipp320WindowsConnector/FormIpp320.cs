using Com.Netpay.Pinpad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Ipp320WindowsConnector
{
    public partial class Form1 : Form
    {
        private static readonly string user = "netejv4c8tjjc";
        private static readonly string password = "hcLICmqxnQ2yCeBN";
        private static readonly string storeId = "18059";
        private static readonly string terminalId = "00010001";
        private static readonly string timeOut = "30000";

        private static readonly string portName = "COM5";

        private static Ipp320Manager ipp320Manager;
        private static string pan;
        private static bool isPartial = false;
        private static bool isReadCustomCard = false;
        private static bool isPartialFinished = true;
        private static string amount;
        private static string transactionType;
        private static string orderId;
        private static string transactionId;

        private static readonly double MAX_VALUE = 21473999.99;

        private static readonly string FILENAME = "./Log_Ipp320.txt";
        private static FileStream ostrm;
        private static StreamWriter writer;
        private static TextWriter oldOut;

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
            ipp320Manager = new Ipp320Manager(Ipp320Manager.NetpayHostType.SANDBOX, user, password, storeId, portName, false);
            //ipp320Manager.StartPCLService();

            // log file
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

            ipp320Manager.useTLS1_2 = false;
            Console.WriteLine("Use TLS1.2 " + ipp320Manager.useTLS1_2);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //ipp320Manager.StopPCLService();
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
                ipp320Manager.GetVersion();
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
            if (isPartial)
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
                ipp320Manager.ProcessTransaction(orderId, amount, Ipp320Manager.PromotionType.a3MSI, "comentario", "DSVRG");
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
            else if ((!isPartial || !isReadCustomCard) && ipp320Manager.PtResponseCode.Equals("00"))
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
                ipp320Manager.LoadInitKey();
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
                ipp320Manager.TimeOutReadCard = 10; // en segundos
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
                    backgroundWorker1.RunWorkerAsync();
                }
            }
        }

        private void RefundEvent(object senders, EventArgs e)
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
                transactionId = this.txtTransactionId.Text;

                if (orderId == null || orderId.Trim().Length == 0)
                {
                    MessageBox.Show("Agregue un id de orden");
                    return;
                }
                else if (transactionId == null || transactionId.Trim().Length == 0)
                {
                    MessageBox.Show("Agregue un id de transaccion");
                    return;
                }
                else
                {

                    transactionType = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Value;
                    ipp320Manager.RefundTransaction(orderId, transactionId, amount,"Procesando...");
                    DisplayResponse();
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