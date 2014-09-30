using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Html5Upload
{
    public partial class savefile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            System.Threading.Thread.Sleep(1000);
            Response.AddHeader("Content-Type", "application/json");

            if (Request.QueryString["resumableChunkNumber"] != null)
            {
                string filedir = Request.MapPath("temp/");
                string savefile =  filedir+ Request.QueryString["resumableChunkNumber"] + ".lzp";
                //Request.Files[0].SaveAs(savefile);
                byte[] data = Request.BinaryRead(Request.ContentLength);
                using (Stream file = File.Create(savefile))
                {
                    file.Write(data, 0, data.Length);
                    file.Close();
                }

                if (Request.QueryString["resumableChunkNumber"] == Request.QueryString["resumableTotalChunks"])
                {
                    MergeFile(filedir, ".lzp", Request.QueryString["resumableFilename"]);
                }
            }
            
            Response.Write("die('{\"jsonrpc\" : \"2.0\", \"result\" : null, \"id\" : \"id\"}');");
            
        }

        /// <summary>
        /// 要合并的文件夹目录
        /// </summary>
        /// <param name="filePath">文件目录</param>
        /// <param name="Extension">扩展名</param>
        /// <param name="filename">合并文件名</param>
        bool MergeFile(string filePath, string Extension,string filename)
        {
            bool rBool = false;
            //获得当前目录下文件夹列表，按文件名排序
            SortedList<int, string> FileList = new SortedList<int, string>();
            DirectoryInfo dirInfo = new DirectoryInfo(filePath);

            foreach (FileSystemInfo var in dirInfo.GetFileSystemInfos())
            {
                if (var.Attributes != FileAttributes.Directory)
                {
                    if (var.Extension == Extension)
                    {
                        FileList.Add(Convert.ToInt32(var.Name.Replace(Extension, "")), var.Name);
                    }
                }
            }

            if (FileList.Count > 0) //存在文件
            {
                FileStream outFile = new FileStream(filePath + "\\" + filename, FileMode.OpenOrCreate, FileAccess.Write);
                using (outFile)
                {
                    foreach (int item in FileList.Keys)
                    {
                        int data = 0;
                        byte[] buffer = new byte[1024];

                        FileStream inFile = new FileStream(filePath + "\\" + FileList[item], FileMode.OpenOrCreate, FileAccess.Read);
                        using (inFile)
                        {
                            while ((data = inFile.Read(buffer, 0, 1024)) > 0)
                            {
                                outFile.Write(buffer, 0, data);
                            }
                            inFile.Close();
                        }
                    }
                    outFile.Close();
                    rBool = true; //合并成功
                }
            }

            return rBool;
        }
    }
}