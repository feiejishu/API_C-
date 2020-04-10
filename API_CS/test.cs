using System;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;

/**
 * @author seeing
 */
namespace Demo
{

    class test
    {
        //**********************请先填打印机编号和KEY，再测试**************************
        public static string USER = "xxxxxxxxxxxx";  //*必填*：登录管理后台的账号名
        public static string UKEY = "xxxxxxxxxxxxxx";//*必填*: 注册账号后生成的UKEY
        public static string SN = "xxxxxxxx";        //*必填*：打印机编号，必须要在管理后台里手动添加打印机或者通过API添加之后，才能调用API

        public static string URL = "http://api.feieyun.cn/Api/Open/";//不需要修改


        //**********测试时，打开下面注释掉方法的即可,更多接口文档信息,请访问官网开放平台查看**********
        static void Main(string[] args)
        {
            //==================添加打印机接口（支持批量）==================
            //***返回值JSON字符串***
            //正确例子：{"msg":"ok","ret":0,"data":{"ok":["sn#key#remark#carnum","316500011#abcdefgh#快餐前台"],"no":["316500012#abcdefgh#快餐前台#13688889999  （错误：识别码不正确）"]},"serverExecutedTime":3}
            //错误：{"msg":"参数错误 : 该帐号未注册.","ret":-2,"data":null,"serverExecutedTime":37}

            //提示：打印机编号(必填) # 打印机识别码(必填) # 备注名称(选填) # 流量卡号码(选填)，多台打印机请换行（\n）添加新打印机信息，每次最多100行(台)。
            //string snlist = "sn1#key1#remark1#carnum1\nsn2#key2#remark2#carnum2";
            //string method = addprinter(snlist);
            //System.Console.WriteLine(method);



            //==================方法1.打印订单==================
            //***返回值JSON字符串***
            //成功：{"msg":"ok","ret":0,"data":"xxxxxxx_xxxxxxxx_xxxxxxxx","serverExecutedTime":5}
            //失败：{"msg":"错误描述","ret":非0,"data":"null","serverExecutedTime":5}

            //string method1 = print();
            //System.Console.WriteLine(method1);


            //===========方法2.查询某订单是否打印成功=============
            //***返回值JSON字符串***
            //成功：{"msg":"ok","ret":0,"data":true,"serverExecutedTime":2}//data:true为已打印,false为未打印
            //失败：{"msg":"错误描述","ret":非0, "data":null,"serverExecutedTime":7}

            //string orderindex = "xxxxxx_xxxxxxx_xxxxxxxxxx";//订单ID，从方法1返回值中获取
            //string method2 = queryOrderState(orderindex);
            //System.Console.WriteLine(method2);


            //===========方法3.查询指定打印机某天的订单详情============
            //***返回值JSON字符串***
            //成功：{"msg":"ok","ret":0,"data":{"print":6,"waiting":1},"serverExecutedTime":9}//print已打印，waiting为打印
            //失败：{"msg":"错误描述","ret":非0,"data":"null","serverExecutedTime":5}

            //string date = "2017-01-06";//注意时间格式为"yyyy-MM-dd"
            //string method3 = queryOrderInfoByDate(date);
            //System.Console.WriteLine(method3);

            //===========方法4.查询打印机的状态==========================
            //***返回值JSON字符串***
            //成功：{"msg":"ok","ret":0,"data":"状态","serverExecutedTime":4}
            //失败：{"msg":"错误描述","ret":非0,"data":"null","serverExecutedTime":5}

            //string method4 = queryPrinterStatus();
            //System.Console.WriteLine(method4);


        }


        //====================以下是函数实现部分==========================
        
        private static string addprinter(string snslist)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
            req.Method = "POST";

            UTF8Encoding encoding = new UTF8Encoding();

            string postData = "printerContent=" + snslist;

            int itime = DateTimeToStamp(System.DateTime.Now);//时间戳秒数
            string stime = itime.ToString();
            string sig = sha1(USER, UKEY, stime);

            //公共参数
            postData += ("&user=" + USER);
            postData += ("&stime=" + stime);
            postData += ("&sig=" + sig);
            postData += ("&apiname=" + "Open_printerAddlist");

            byte[] data = encoding.GetBytes(postData);

            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            Stream resStream = req.GetRequestStream();

            resStream.Write(data, 0, data.Length);
            resStream.Close();

            HttpWebResponse response;
            string strResult;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                strResult = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                strResult = response.StatusCode.ToString();//错误信息
            }

            response.Close();
            req.Abort();

            return strResult;
        }


        //方法1
        private static string print()
        {
            //标签说明：
            //单标签: 
            //"<BR>"为换行,"<CUT>"为切刀指令(主动切纸,仅限切刀打印机使用才有效果)
            //"<LOGO>"为打印LOGO指令(前提是预先在机器内置LOGO图片),"<PLUGIN>"为钱箱或者外置音响指令
            //成对标签：
            //"<CB></CB>"为居中放大一倍,"<B></B>"为放大一倍,"<C></C>"为居中,<L></L>字体变高一倍
            //<W></W>字体变宽一倍,"<QR></QR>"为二维码,"<BOLD></BOLD>"为字体加粗,"<RIGHT></RIGHT>"为右对齐

            //拼凑订单内容时可参考如下格式
            string orderInfo;
            orderInfo = "<CB>测试打印</CB><BR>";//标题字体如需居中放大,就需要用标签套上
            orderInfo += "名称　　　　　 单价  数量 金额<BR>";
            orderInfo += "--------------------------------<BR>";
            orderInfo += "番　　　　　　 1.0    1   1.0<BR>";
            orderInfo += "番茄　　　　　 10.0   10  10.0<BR>";
            orderInfo += "番茄炒　　　　 10.0   100 100.0<BR>";
            orderInfo += "番茄炒粉　　　 100.0  100 100.0<BR>";
            orderInfo += "番茄炒粉粉　　 1000.0 1   100.0<BR>";
            orderInfo += "番茄炒粉粉粉粉 100.0  100 100.0<BR>";
            orderInfo += "番茄炒粉粉粉粉 15.0   1   15.0<BR>";
            orderInfo += "备注：快点送到<BR>";
            orderInfo += "--------------------------------<BR>";
            orderInfo += "合计：xx.0元<BR>";
            orderInfo += "送货地点：xxxxxxxxxxxxxxxxx<BR>";
            orderInfo += "联系电话：138000000000<BR>";
            orderInfo += "订餐时间：2011-01-06 19:30:10<BR>";
            orderInfo += "----------请扫描二维码----------";
            orderInfo += "<QR>http://www.dzist.com</QR>";//把二维码字符串用标签套上即可自动生成二维码
            orderInfo += "<BR>";

            orderInfo = Uri.EscapeDataString(orderInfo);
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
            req.Method = "POST";
            UTF8Encoding encoding = new UTF8Encoding();

            string postData = "sn=" + SN;
            postData += ("&content=" + orderInfo);
            postData += ("&times=" + "1");//默认1联

            int itime = DateTimeToStamp(System.DateTime.Now);//时间戳秒数
            string stime = itime.ToString();
            string sig = sha1(USER, UKEY, stime);

            //公共参数
            postData += ("&user=" + USER);
            postData += ("&stime=" + stime);
            postData += ("&sig=" + sig);
            postData += ("&apiname=" + "Open_printMsg");

            byte[] data = encoding.GetBytes(postData);

            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            Stream resStream = req.GetRequestStream();

            resStream.Write(data, 0, data.Length);
            resStream.Close();

            HttpWebResponse response;
            string strResult;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                strResult = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                strResult = response.StatusCode.ToString();//错误信息
            } 

			response.Close();
            req.Abort();
            //服务器返回的JSON字符串，建议要当做日志记录起来
            return strResult;

        }


        //方法2
        private static string queryOrderState(string orderid)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
            req.Method = "POST";

            UTF8Encoding encoding = new UTF8Encoding();

            string postData = "orderid=" + orderid;

            int itime = DateTimeToStamp(System.DateTime.Now);//时间戳秒数
            string stime = itime.ToString();
            string sig = sha1(USER, UKEY, stime);

            //公共参数
            postData += ("&user=" + USER);
            postData += ("&stime=" + stime);
            postData += ("&sig=" + sig);
            postData += ("&apiname=" + "Open_queryOrderState");

            byte[] data = encoding.GetBytes(postData);

            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            Stream resStream = req.GetRequestStream();

            resStream.Write(data, 0, data.Length);
            resStream.Close();

            HttpWebResponse response;
            string strResult;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                strResult = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                strResult = response.StatusCode.ToString();//错误信息
            }

            response.Close();
            req.Abort();

            return strResult;
        }

        //方法3
        private static string queryOrderInfoByDate(string strdate)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
            req.Method = "POST";

            UTF8Encoding encoding = new UTF8Encoding();

            string postData = "sn=" + SN;
            postData += ("&date=" + strdate);

            int itime = DateTimeToStamp(System.DateTime.Now);//时间戳秒数
            string stime = itime.ToString();
            string sig = sha1(USER, UKEY, stime);

            //公共参数
            postData += ("&user=" + USER);
            postData += ("&stime=" + stime);
            postData += ("&sig=" + sig);
            postData += ("&apiname=" + "Open_queryOrderInfoByDate");

            byte[] data = encoding.GetBytes(postData);

            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            Stream resStream = req.GetRequestStream();

            resStream.Write(data, 0, data.Length);
            resStream.Close();

            HttpWebResponse response;
            string strResult;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                strResult = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                strResult = response.StatusCode.ToString();//错误信息
            }

            response.Close();
            req.Abort();

            return strResult;
        }


        //方法4
        private static string queryPrinterStatus()
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
            req.Method = "POST";

            UTF8Encoding encoding = new UTF8Encoding();

            string postData = "sn=" + SN;

            int itime = DateTimeToStamp(System.DateTime.Now);//时间戳秒数
            string stime = itime.ToString();
            string sig = sha1(USER, UKEY, stime);

            //公共参数
            postData += ("&user=" + USER);
            postData += ("&stime=" + stime);
            postData += ("&sig=" + sig);
            postData += ("&apiname=" + "Open_queryPrinterStatus");

            byte[] data = encoding.GetBytes(postData);

            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            Stream resStream = req.GetRequestStream();

            resStream.Write(data, 0, data.Length);
            resStream.Close();

            HttpWebResponse response;
            string strResult;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                strResult = reader.ReadToEnd();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                strResult = response.StatusCode.ToString();//错误信息
            }

            response.Close();
            req.Abort();

            return strResult;

        }


        //签名USER,UKEY,STIME
        public static string sha1(string user, string ukey, string stime) {
            var buffer = Encoding.UTF8.GetBytes(user+ ukey+ stime);
            var data = SHA1.Create().ComputeHash(buffer);

            var sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString().ToLower();

        }


        private static int DateTimeToStamp(System.DateTime time) {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); return (int)(time - startTime).TotalSeconds;
        }



    }


}
