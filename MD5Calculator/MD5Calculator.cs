// (C) 2004 André Betz
// http://www.andrebetz.de
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Threading;

namespace MD5Calculator
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MD5Calculator : System.Windows.Forms.Form
	{
		#region Member Variables
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader FileName;
		private System.Windows.Forms.ColumnHeader Test;
		private string DirectoryPath = null;
		private string TestFileName = null;
		private string[] TestFiles = null;
		private Thread MD5Thread = null;
		private Thread MD5ProgressThread = null;
		private ProgressViewer pvw = null;
		private System.ComponentModel.Container components = null;
		#endregion
		#region AppWizardThings
		public MD5Calculator()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MD5Calculator());
		}

		#endregion
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.listView1 = new System.Windows.Forms.ListView();
			this.FileName = new System.Windows.Forms.ColumnHeader();
			this.Test = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(8, 16);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(104, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Choose Directory";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(8, 48);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(104, 23);
			this.button2.TabIndex = 1;
			this.button2.Text = "Choose File";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(8, 80);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(104, 23);
			this.button3.TabIndex = 2;
			this.button3.Text = "Create MD5";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(8, 112);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(104, 23);
			this.button4.TabIndex = 3;
			this.button4.Text = "Test MD5";
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.FileName,
																						this.Test});
			this.listView1.Location = new System.Drawing.Point(120, 16);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(360, 120);
			this.listView1.TabIndex = 4;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// FileName
			// 
			this.FileName.Text = "FileName";
			this.FileName.Width = 270;
			// 
			// Test
			// 
			this.Test.Text = "TestResult";
			this.Test.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.Test.Width = 75;
			// 
			// MD5Calculator
			// 
			this.AutoScale = false;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(490, 151);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MD5Calculator";
			this.Text = "MD5Calculator www.andrebetz.de";
			this.ResumeLayout(false);

		}
		#endregion
		#region ButtonHandlers

		private void button1_Click(object sender, System.EventArgs e)
		{
			FolderBrowserDialog fbd= new FolderBrowserDialog();
			if(fbd.ShowDialog(this) == DialogResult.OK)
			{
				TestFileName = null;
				DirectoryPath = fbd.SelectedPath;
			}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			if(ofd.ShowDialog(this) == DialogResult.OK)
			{
				DirectoryPath = null;
				TestFileName = ofd.FileName;
			}
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			TestFiles = null;
			if(DirectoryPath!=null)
			{
				try
				{
					TestFiles =	Directory.GetFiles(DirectoryPath);
				}
				catch
				{
					return;
				}
			}
			else if(TestFileName!=null)
			{
				TestFiles = new string[1];
				TestFiles[0] = TestFileName;
			}
			else
			{
				MessageBox.Show("No File or Directory selected");
				return;
			}
			StartProgressViewer();
			StartCalculateMD5();
		}

		private void button4_Click(object sender, System.EventArgs e)
		{
			TestFiles = null;

			if(DirectoryPath!=null)
			{
				try
				{
					TestFiles =	Directory.GetFiles(DirectoryPath);
				}
				catch
				{
					return;
				}
			}
			else if(TestFileName!=null)
			{
				TestFiles = new string[1];
				TestFiles[0] = TestFileName;
			}
			else
			{
				MessageBox.Show("No File or Directory selected");
				return;
			}
			StartProgressViewer();
			StartValidateMD5();
		}
		#endregion
		#region PrivateMethods
		private void StartProgressViewer()
		{
			pvw = new ProgressViewer();
			MD5ProgressThread = new Thread(new ThreadStart(ShowProgressViewer));
			MD5ProgressThread.Name = "View ProgressBar";
			MD5ProgressThread.Start();
		}
		private void ShowProgressViewer()
		{
			pvw.ShowDialog(this);
			pvw = null;
		}
		private void StartCalculateMD5()
		{
			this.listView1.Items.Clear();
			MD5Thread = new Thread(new ThreadStart(CalculateMD5Thread));
			MD5Thread.Name = "Calculate MD5";
			MD5Thread.Start();
		}
		private void CalculateMD5Thread()
		{
			this.button1.Enabled = false;
			this.button2.Enabled = false;
			this.button3.Enabled = false;
			this.button4.Enabled = false;
			MD5Algo ma = new MD5Algo(new MD5Algo.Progress(ProgressCallBack));
			foreach(string fn in TestFiles)
			{
				string Extension = Path.GetExtension(fn).ToUpper();

				if(!Extension.Equals(".MD5"))
				{
					pvw.Init();
					string MD5Sum = ma.CalculateMD5(fn);
					if(MD5Sum!=null)
					{
						if(StoreMD5Sum(MD5Sum,fn))
						{
							this.listView1.Items.Add(new ListViewItem(fn));
						}
					}
				}
			}
			pvw.Close();
			this.button1.Enabled = true;
			this.button2.Enabled = true;
			this.button3.Enabled = true;
			this.button4.Enabled = true;
		}
		private void StartValidateMD5()
		{
			this.listView1.Items.Clear();
			MD5Thread = new Thread(new ThreadStart(ValidateMD5Thread));
			MD5Thread.Name = "Validate MD5";
			MD5Thread.Start();
		}
		private void ValidateMD5Thread()
		{
			this.button1.Enabled = false;
			this.button2.Enabled = false;
			this.button3.Enabled = false;
			this.button4.Enabled = false;
			MD5Algo ma = new MD5Algo(new MD5Algo.Progress(ProgressCallBack));
			foreach(string fn in TestFiles)
			{
				if(!Path.GetExtension(fn).ToUpper().Equals(".MD5"))
				{
					pvw.Init();
					string MD5Sum = ma.CalculateMD5(fn);
					string MD5SumOrig = ReadMD5Sum(fn);
					if(MD5SumOrig!=null)
					{
						string[] Result = new string[2];
						Result[0] = fn;
						if(MD5SumOrig.Equals(MD5Sum))
						{
							Result[1] = "OK";
						}
						else
						{
							Result[1] = "Error";
						}
						this.listView1.Items.Add(new ListViewItem(Result));
					}
				}
			}
			pvw.Close();
			this.button1.Enabled = true;
			this.button2.Enabled = true;
			this.button3.Enabled = true;
			this.button4.Enabled = true;
		}
		private bool StoreMD5Sum(string MD5Sum,string fn)
		{
			string NewFN = Path.GetDirectoryName(fn)+"\\"+Path.GetFileNameWithoutExtension(fn) + ".md5";
			if(File.Exists(NewFN))
			{
				File.Delete(NewFN);
			}

			try
			{
				FileStream ArchivFile = new FileStream(NewFN,FileMode.OpenOrCreate,FileAccess.Write);
				byte[] btAr = cpyChar2Byte(MD5Sum.ToCharArray());
				ArchivFile.Write(btAr,0,btAr.Length);
				ArchivFile.Flush();
				ArchivFile.Close();
			}
			catch
			{
				return false;
			}

			return true;
		}
		private string ReadMD5Sum(string fn)
		{
			string MD5Sum = null;
			string NewFN = Path.GetDirectoryName(fn)+"\\"+Path.GetFileNameWithoutExtension(fn) + ".md5";
			if(!File.Exists(NewFN))
			{
				return null;
			}
			try
			{
				FileStream Datei = new FileStream(NewFN,FileMode.Open,FileAccess.Read);
				long len = Datei.Length;
				byte[] MD5Arr = new byte[len];
				long readlen = Datei.Read(MD5Arr,0,MD5Arr.Length);
				Datei.Close();
				if(readlen!=len)
				{
					return null;
				}
				MD5Sum = cpyByte2String(MD5Arr);
			}
			catch
			{
				return null;
			}
			return MD5Sum;
		}
		private byte[] cpyChar2Byte(char[] chArr)
		{
			byte[] btArr = new byte[chArr.Length];
			for(int i=0;i<btArr.Length;i++)
			{
				btArr[i] = (byte)chArr[i];
			}
			return btArr;
		}
		private string cpyByte2String(byte[] btArr)
		{
			string ResStr = "";
			for(int i=0;i<btArr.Length;i++)
			{
				ResStr += (char)btArr[i];
			}
			return ResStr;
		}
		public void ProgressCallBack(int Steps)
		{
			if(pvw!=null)
			{
				pvw.Step(Steps);
			}
		}
		#endregion
	}
}
