using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
//using WPeInvoiceDll;


namespace WpTools
{
    public partial class FormWPeInvoiceTest : Form
    {
        
        int RunTestCnt = 0;
        byte QrCdoeWidth = 3;
        static SerialPort com = new SerialPort();

        public FormWPeInvoiceTest()
        {
            InitializeComponent();
        }

        private void CallByWPeInvoice()
        {
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            btnPrint.Enabled = false;
            buttonRunTest.Enabled = false;

            //CallByWPeInvoice();
            CallByCommandCode();

            btnPrint.Enabled = true;
            buttonRunTest.Enabled = true;
        }

        // ---------------------------------------------------------------------------------
        private void CallByCommandCode()
        {
            // printer command define ------------------------------------------------------
            const string SelectPageMode = "\x1B" + "\x2";
            const string PrintAndCancelPageMode = "\x1B" + "\x3";
            const string PrintNvImage = "\x1C" + "p" + "\x1" + "\x0";

            // Position Control Functions ------------------------------------------------------
            const string SetAbsolutePrintPosition16Dots = "\x1B" + "$" + "\x10" + "\x0";
            const string SetAbsolutePrintPosition40Dots = "\x1B" + "$" + "\x28" + "\x0";
            byte[] SetAbsolutePrintPosition208Dots = { 0x1B, (byte)'$', 208, 0 };
            byte[] SetAbsolutePrintPosition248Dots = { 0x1B, (byte)'$', 248, 0 };
 
            // cut Control Functions ------------------------------------------------------
            const string PartialCut1 = "\x1B" + "m";
            const string FeedPartialCut1 = "\x1D" + "VB" + "\x0";

            // Font Control Functions ------------------------------------------------------
            const string SelectGlobalFontW1H1 = "\x1D" + "!" + "\x0";
            const string SelectGlobalFontW1H2 = "\x1D" + "!" + "\x01";
            const string SelectGlobalFontW2H2 = "\x1D" + "!" + "\x11";
            const string SelectFontBoldMode = "\x1B" + "E" + "\x1";
            const string CancelFontBoldMode = "\x1B" + "E" + "\x0";

            // Paper Feed Functions ------------------------------------------------------
            const string PaperFeed24Dots = "\x1B" + "J" + "\x18";
            const string PaperFeed12Dots = "\x1B" + "J" + "\x0C";
            const string PaperBackFeed100Dots = "\x1B" + "K" + "\x64";
            const string PaperBackFeed31Dots = "\x1B" + "K" + "\x1F";
            const string PaperBackFeed72Dots = "\x1B" + "K" + "\x48";

            // Align Format Functions ------------------------------------------------------
            const string SelectAlignLeft = "\x1B" + "a" + "\x0";
            const string SelectAlignCenter = "\x1B" + "a" + "\x1";
            const string SelectAlignRight = "\x1B" + "a" + "\x2";

            // 1D Barcode Function -----------------------------------------------------------
            const string SelectBarCodeWidthDot1 = "\x1D" + "w" + "\x1";
            const string SelectBarCodeHighDot48 = "\x1D" + "h" + "\x32";
            const string PrnBarCode39EndByNull = "\x1D" + "k" + "\x4";

            // QR Code Function -----------------------------------------------------------
            const string SelectQrCodeSize3Dot = "\x1D" + "(k" + "\x3\x0\x31\x43\x3";
            const string SelectQrCodeSize4Dot = "\x1D" + "(k" + "\x3\x0\x31\x43\x4";
            const string SelectQrCodeErrorCorrectionLevelL = "\x1D" + "(k" + "\x3\x0\x31\x45\x30";
            const string SelectQrCodeVersion6 = "\x1D" + "(k" + "\x3\x0\x31\x76\x6";
            const string QrCodeDataStoreLeading = "\x1D" + "(k" + "\x3\x0\x31\x50\x30";
            const string QrDataPrint = "\x1D" + "(k" + "\x3\x0\x31\x51\x30";

            // initial com parameter ------------------------------------------------------
            com.PortName = cboComPort.Text;
            com.BaudRate = int.Parse(cboBaudRate.Text);
            com.Parity = Parity.None;
            com.DataBits = 8;
            com.StopBits = StopBits.One;
            com.Handshake = Handshake.None;

            string caption = "COM Port Error";
            string message = "";

            // check com hardware handshaking-----------------------------------------------
            com.Open();
            if (com.CtsHolding == false)
            {
                message = "COM CTS not ready ???";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                goto Exit;
            }

            if (com.DsrHolding == false)
            {
                message = "COM DSR not ready ???";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                goto Exit;
            }

            // 以下資料需在整頁模式運作 -----------------------------------------------
            string ss = "";
            ss += SelectPageMode;               //整頁模式

            ss += SelectAlignCenter;            //文字置中
            ss += PrintNvImage;                 //列印logo, logo須事先下載, 若無logo則忽略
            ss += SelectFontBoldMode;           //文字加粗
            ss += SelectGlobalFontW2H2;         //文字兩倍寬兩倍高
            ss += "\n\n\n\n";                   //送行
            ss += "電子發票證明聯\n";

            if (QrCdoeWidth == 3)
                ss += PaperFeed24Dots;          //送紙24點, 藉由調整點數來調整頁長
            else
                ss += PaperFeed12Dots;          //送紙12點, 藉由調整點數來調整頁長
                
            ss += "102年08-09月\n";

            if (QrCdoeWidth == 3)
                ss += PaperFeed24Dots;          //送紙24點, 藉由調整點數來調整頁長
            else
                ss += PaperFeed12Dots;          //送紙12點, 藉由調整點數來調整頁長

            ss += "AB-12345678\n";

            if (QrCdoeWidth == 3)
                ss += PaperFeed24Dots;          //送紙24點, 藉由調整點數來調整頁長
            else
                ss += PaperFeed12Dots;          //送紙12點, 藉由調整點數來調整頁長

            ss += SelectAlignLeft;              //內容靠左
            ss += SelectGlobalFontW1H1;         //文字回復正常
            ss += CancelFontBoldMode;           //文字取消加粗
            ss += "2013-08-22 15:14:39  格式 25\n";
            ss += "隨機碼:7579         總計:$80\n";
            ss += "賣  方:12345678\n";

            // process code39 -----------------------------------------------
            ss += SelectAlignCenter;            //內容置中
            ss += SelectBarCodeWidthDot1;       //選擇一維條碼粗細 -- 1 dot
            ss += SelectBarCodeHighDot48;       //選擇一維條碼高度 -- 64 dot
            ss += PrnBarCode39EndByNull;        //列印code39之指令前置碼
            ss += "1234567890123456789";        //列印code39之內容 -- 19 碼
            ss += "\x0";                        //列印code39之指令結束碼 null
            ss += PaperFeed12Dots;              //送紙12點

            // process QR Code -----------------------------------------------
            ss += SelectQrCodeVersion6;                 //選擇 QR Code 版本固定於 -- 6

            if (QrCdoeWidth == 3)
                ss += SelectQrCodeSize3Dot;             //選擇 QR Code 粗細 -- 3 dot
            else
                ss += SelectQrCodeSize4Dot;             //選擇 QR Code 粗細 -- 4 dot

            ss += SelectQrCodeErrorCorrectionLevelL;    //選擇 QR Code 糾錯等級 -- L

            byte[] bb = System.Text.Encoding.GetEncoding("Big5").GetBytes(ss);
            com.Write(bb, 0, bb.Length);
            bb.Initialize();

            // process left side QR Code -----------------------------------------------
            ss = SelectAlignLeft;                           //內容靠左
            ss += SetAbsolutePrintPosition16Dots;           //左邊QrCode靠左16點
            bb = System.Text.Encoding.GetEncoding("Big5").GetBytes(ss);
            com.Write(bb, 0, bb.Length);
            bb.Initialize();

            string sqr1 = "";
            sqr1 += QrCodeDataStoreLeading;                 //儲存 QR Code Data 指令前置碼
            sqr1 += "AB123451101020822757";
            sqr1 += "90000000000000050000";
            sqr1 += "0000012345678W7TZdCv";
            sqr1 += "fOm6QnYHxODFV3A==:**";
            sqr1 += "********:5:5:0:";
            sqr1 += "葡萄柚綠茶:1:10";                      // QR Code Data for version 6 -- 建議 max. 130 bytes
                                                            // 資料不足不需補空白
            bb = System.Text.Encoding.GetEncoding("Big5").GetBytes(sqr1);
            bb[3] = (byte)((bb.Length - 5) & 0xFF);         // 修正指令之參數 length -- low byte 
            bb[4] = (byte)((bb.Length - 5) >> 8);           // 修正指令之參數 length -- high byte 
            com.Write(bb, 0, bb.Length);
            bb.Initialize();

            ss = "";
            ss += QrDataPrint;                              //列印 QR Code Data
            bb = System.Text.Encoding.GetEncoding("Big5").GetBytes(ss);
            com.Write(bb, 0, bb.Length);
            bb.Initialize();

            // process right side QR Code -----------------------------------------------
            ss = SelectAlignLeft;                           //內容靠左
            ss += PaperBackFeed100Dots;                     //指針倒退 -- 100 dots
            if (QrCdoeWidth == 3)
                ss += PaperBackFeed31Dots;                  //指針倒退 -- 31 dots, 合計倒退 -- 131 dots
            else
                ss += PaperBackFeed72Dots;                  //指針倒退 -- 72 dots, 合計倒退 -- 172 dots

            bb = System.Text.Encoding.GetEncoding("Big5").GetBytes(ss);
            com.Write(bb, 0, bb.Length);
            bb.Initialize();

            //右邊QrCode靠左208點
            com.Write(SetAbsolutePrintPosition208Dots, 0, SetAbsolutePrintPosition208Dots.Length);

            string sqr2 = "";
            sqr2 += QrCodeDataStoreLeading;                 //儲存 QR Code Data 指令前置碼
            sqr2 += "葡萄柚紅茶:1:20"; 
            sqr2 += "烏龍柚香茶:1:30";
            sqr2 += "烏龍梅子茶:1:40";
            sqr2 += "烏龍百香茶:1:50";                      // QR Code Data for version 6 -- 建議 max. 130 bytes
                                                            // 資料不足不需補空白
            bb = System.Text.Encoding.GetEncoding("Big5").GetBytes(sqr2);
            bb[3] = (byte)((bb.Length - 5) & 0xFF);         // 修正指令之參數 length -- low byte 
            bb[4] = (byte)((bb.Length - 5) >> 8);           // 修正指令之參數 length -- high byte 
            com.Write(bb, 0, bb.Length);
            bb.Initialize();

            ss = "";
            ss += QrDataPrint;                              //列印 QR Code Data
            ss += PrintAndCancelPageMode;                   //列印並取消整頁模式
            bb = System.Text.Encoding.GetEncoding("Big5").GetBytes(ss);
            com.Write(bb, 0, bb.Length);
            bb.Initialize();

            ss = "";
            ss += "\n\n";                                   //送行
            ss += SelectAlignLeft;                          //內容靠左
            ss += "發財店(001) 序:123\n";
            ss += PartialCut1;                              //切紙
            bb = System.Text.Encoding.GetEncoding("Big5").GetBytes(ss);
            com.Write(bb, 0, bb.Length);
            bb.Initialize();

            // 以下一般資料不需要在整頁模式運作 -----------------------------------------------

            ss = "";
            //ss += PaperFeed12Dots;                          //送紙12點
            ss += "WinPOS POS\n";
            ss += "TEL:02-86657955  FAX:02-86659446\n";
            ss += "\n";
            ss += "葡萄柚綠茶                   10X\n";
            ss += "葡萄柚紅茶                   20X\n";
            ss += "烏龍柚香茶                   30X\n";
            ss += "烏龍梅子茶                   40X\n";
            ss += "烏龍百香茶                   50X\n";
            ss += "付現:150 找零:0\n";
            ss += SelectGlobalFontW1H2;                     //文字一倍寬兩倍高
            ss += "共 4項 合計 $150\n";
            ss += "\n";
            ss += SelectGlobalFontW1H1;                     //文字回復正常
            ss += "應稅額:76 免稅額:0 稅額:4\n";
            ss += FeedPartialCut1;                          //送紙及切紙

            bb = System.Text.Encoding.GetEncoding("Big5").GetBytes(ss);
            com.Write(bb, 0, bb.Length);
        Exit:
            com.Close();
        }

        private void cboComPort_Click(object sender, EventArgs e)
        {
            cboComPort.Items.Clear();
            foreach (string s in SerialPort.GetPortNames()) { cboComPort.Items.Add(s); }
        }

        private void cboQrCodeWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            QrCdoeWidth = byte.Parse(cboQrCodeWidth.Text);
        }

        // ---------------------------------------------------------------------------------
        private void FormWPeInvoiceTest_Load(object sender, EventArgs e)
        {

        }

        // ---------------------------------------------------------------------------------
        private void timer1_Tick(object sender, EventArgs e)
        {
            CallByCommandCode();
            RunTestCnt++;
            textBoxRunCounter.Text = RunTestCnt.ToString();
            if (RunTestCnt < int.Parse(comboBoxRunCntSel.Text)) return;

            timer1.Enabled = false;
            buttonRunTest.Text = "Run Test Start";
            btnPrint.Enabled = true;
        }

        // ---------------------------------------------------------------------------------
        private void buttonRunTest_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled == false)
            {
                timer1.Enabled = true;
                buttonRunTest.Text = "Run Test Stop";
                btnPrint.Enabled = false;
                RunTestCnt = 0;
            }
            else
            {
                timer1.Enabled = false;
                buttonRunTest.Text = "Run Test Start";
                btnPrint.Enabled = true;
            }
        }

        private void cboComPort_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
