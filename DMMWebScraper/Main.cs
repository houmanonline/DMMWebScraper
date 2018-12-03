using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DMMWebScraper
{
    public partial class Main : Form
    {
        Int32 imgHeight;
        Int32 imgWidth;
        MoviesDetail movieDetail = new MoviesDetail();
        List<PictureBox> pictureboxes = new List<PictureBox>();
        ScreenShots screenShotForm = new ScreenShots();

        public Main()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }
        // separate letters and numbers in the string
        private string[] SeparateLetterAndNumber(string str)
        {
            var array = Regex.Matches(str, @"\D+|\d+")
                             .Cast<Match>()
                             .Select(m => m.Value)
                             .ToArray();
            return array;
        }
        // In DMM.co.jp, ssni054 is ssni00054 in the URL, so add zero in front.
        private string AddZeroForDigital(string str)
        {
            if (str.Length < 2)
            {
                str = "0000" + str;
            }
            else if (str.Length < 3)
            {
                str = "000" + str;
            }
            else if (str.Length < 4)
            {
                str = "00" + str;
            }
            return str;
        }
        // If user input BHG-20, it should be BHG-020
        private string AddZeroForDVD(string str)
        {
            if (str.Length < 2)
            {
                str = "00" + str;
            }
            else if (str.Length < 3)
            {
                str = "0" + str;
            }
            return str;
        }
        // Handle the serial NO which is input by user
        private string DealWithDigitalSerialNo(string SerialNo)
        {
            string[] DigitalSerialNoArray = null;
            if (SerialNo.Contains(" "))
            {
                DigitalSerialNoArray = SerialNo.Split(' ');
            }
            else if (SerialNo.Contains("-"))
            {
                DigitalSerialNoArray = SerialNo.Split('-');
            }
            else
            {
                DigitalSerialNoArray = SeparateLetterAndNumber(SerialNo);
            }
            
            int i = DigitalSerialNoArray.Length;
            DigitalSerialNoArray[i - 1] = AddZeroForDigital(DigitalSerialNoArray[i - 1]);
          
            return String.Join(string.Empty, DigitalSerialNoArray);
        }
        // Handle the serial NO which is input by user
        private string DealWithDVDSerialNo(string SerialNo)
        {
            string[] DigitalSerialNoArray = null;
            if (SerialNo.Contains(" "))
            {
                DigitalSerialNoArray = SerialNo.Split(' ');
            }
            else if (SerialNo.Contains("-"))
            {
                DigitalSerialNoArray = SerialNo.Split('-');
            }
            else
            {
                DigitalSerialNoArray = SeparateLetterAndNumber(SerialNo);
            }

            int i = DigitalSerialNoArray.Length;
            DigitalSerialNoArray[i - 1] = AddZeroForDVD(DigitalSerialNoArray[i - 1]);

            return String.Join(string.Empty, DigitalSerialNoArray);
        }
        //check if the video exists
        private bool CheckExists(HtmlAgilityPack.HtmlDocument doc)
        {
            if (doc.DocumentNode.SelectSingleNode("//span[text()='404 Not Found']") != null)
            {
                return true;
            }
            else { return false; }
        }
        //Download the image from Url        
        private Image DownloadImage(string url)
        {
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData(url);
            MemoryStream ms = new MemoryStream(bytes);
            Image posterImage = Image.FromStream(ms);
            
            return posterImage;
        }
        //open a new form and display poster
        private void PopUpPoster(string imgUrl)
        {
            
            movieDetail.Poster = DownloadImage(imgUrl);
            imgHeight = movieDetail.Poster.Height;
            imgWidth = movieDetail.Poster.Width;
            //create new form and set the width and height
            Poster posterForm = new Poster();
            posterForm.Height = Convert.ToInt32(imgHeight * 1.2);
            posterForm.Width = imgWidth;
            posterForm.StartPosition = FormStartPosition.CenterScreen;
            //add form title
            posterForm.Text = movieDetail.Title;
            //create new picturebox to display poster
            PictureBox pictureBox = new PictureBox();
            pictureBox.Dock = DockStyle.Top;
            pictureBox.Width = imgWidth;
            pictureBox.Height = imgHeight;
            pictureBox.Image = movieDetail.Poster;
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;

            Button btnSave = new Button();
            btnSave.Location = new Point(imgWidth / 4, imgHeight + 10);
            btnSave.Text = "Save";
            btnSave.Click += new EventHandler(this.btnSave_Click);
            posterForm.Controls.Add(btnSave);
            posterForm.Controls.Add(pictureBox);
            

            Button btnSamples = new Button();
            btnSamples.Location = new Point(imgWidth / 4 * 3, imgHeight + 10);
            btnSamples.AutoSize = true;
            btnSamples.Text = "Show Screenshots";
            btnSamples.Click += new EventHandler(this.btnSamples_Click);
            posterForm.Controls.Add(btnSamples);            
            if (movieDetail.IsContainSamples == false)
            {
                btnSamples.Enabled = false;
            }
            posterForm.Controls.Add(pictureBox);
            posterForm.Show();
        }
        //event hanlder of save button in the posterForm
        void btnSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image files (*.jpg) | *.jpg|Image files ( *.png)|*.png";
            sfd.FileName = "[" + movieDetail.SerialNO + "]-" + movieDetail.Title;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                movieDetail.Poster.Save(sfd.FileName);
            }

        }
        //event hanlder of show sample button in the posterForm
        void btnSamples_Click(object sender, EventArgs e)
        {
            Samples samples = new Samples();
            samples.Width = 800;
            samples.Height = 600;

            FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel();
            flowLayoutPanel.Width = 800;
            flowLayoutPanel.Height = 600;
            
            
            for (int i = 0; i < movieDetail.Samples.Count(); i++)
            {
                
                pictureboxes.Add(new PictureBox());
                pictureboxes[i].MouseHover += new EventHandler(this.pictureboxes_MouseHover);
                pictureboxes[i].MouseLeave += new EventHandler(this.pictureboxes_MouseLeave);
                pictureboxes[i].MouseClick += new MouseEventHandler(this.pictureboxes_MouseClick);
                pictureboxes[i].Dock = DockStyle.Top;
                pictureboxes[i].Width = imgWidth;
                pictureboxes[i].Height = imgHeight;
                pictureboxes[i].Image = movieDetail.Samples[i];
                pictureboxes[i].SizeMode = PictureBoxSizeMode.AutoSize;
                //create a name for each picturebox
                pictureboxes[i].Name = i.ToString();
                
                flowLayoutPanel.Controls.Add(pictureboxes[i]);                
            }
            samples.StartPosition = FormStartPosition.CenterScreen;
            samples.Controls.Add(flowLayoutPanel);
            samples.Show();
            //clear the lists
            movieDetail.Samples.Clear();
            pictureboxes.Clear();
        }
        //click to save the screenshot
        private void pictureboxes_MouseClick(object sender, EventArgs e)
        {
            //get the name of mouse over picturebox 
            var picturebox = (PictureBox)sender;
            string s = picturebox.Name;
            //create a variable to store the screenshot
            Image screenShots = movieDetail.ScreenShots[Convert.ToInt32(s)];
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image files (*.jpg) | *.jpg|Image files ( *.png)|*.png";
            sfd.FileName = "[" + movieDetail.SerialNO + "]-" + movieDetail.Title + " ScreenShot" + s;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                screenShots.Save(sfd.FileName);
            }
        }

        //Mouse over screenshot event
        void pictureboxes_MouseHover(object sender, EventArgs e)
        {
            //get the name of mouse over picturebox 
            var picturebox = (PictureBox)sender;
            string s = picturebox.Name;
            //create a variable to store the screenshot
            Image screenShots = movieDetail.ScreenShots[Convert.ToInt32(s)];
            int screenShotWidth = screenShots.Width;
            int screenShotHeight = screenShots.Height;
            //get cursor position 
            this.Cursor = new Cursor(Cursor.Current.Handle);           
            //define a new form
            screenShotForm.Height = screenShotHeight;
            screenShotForm.Width = screenShotWidth;
            //set the position of the screenshot form
            if(Cursor.Position.X < Screen.PrimaryScreen.WorkingArea.Width / 2 || Cursor.Position.Y < Screen.PrimaryScreen.WorkingArea.Height / 2)
            {
                screenShotForm.Location = new Point(Cursor.Position.X + 50, Cursor.Position.Y + 50);
            }
            else
            {
                screenShotForm.Location = new Point(Cursor.Position.X - screenShotWidth, Cursor.Position.Y - screenShotHeight);
            }
            
            //create a new picturebox
            PictureBox screenShotPicturebox = new PictureBox();
            screenShotPicturebox.Height = screenShotHeight;
            screenShotPicturebox.Width = screenShotWidth;
            screenShotPicturebox.Dock = DockStyle.Top;
            screenShotPicturebox.Width = imgWidth;
            screenShotPicturebox.Height = imgHeight;
            screenShotPicturebox.Image = screenShots;
            screenShotPicturebox.SizeMode = PictureBoxSizeMode.AutoSize;
            //display screenshot
            screenShotForm.Controls.Add(screenShotPicturebox);
            screenShotForm.Show();
        }
        //Mouse leave screenshot event
        void pictureboxes_MouseLeave(object sender, EventArgs e)
        {
            screenShotForm.Hide();
        }
        //get the movie info and fill into an instance of MoviesDetail class
        void GetDigitalDetailFromWeb(HtmlAgilityPack.HtmlDocument doc)
        {   //grabbing movie info from web                     
            var videoDetail = doc.DocumentNode.SelectNodes("//table[@class = 'mg-b20']/tr/td/following-sibling::*").ToList();
            //add movie info
            movieDetail.ReleaseDate = videoDetail[2].InnerText.Trim(new char[] { '\n', '\r' });
            movieDetail.Length = videoDetail[3].InnerText.Trim(new char[] { '\n', '\r' });
            string[] actors = videoDetail[4].InnerText.Split('\n');
            for(int i = 0; i < actors.Length; i++)
            {
                if (!string.IsNullOrEmpty(actors[i]))
                {
                    movieDetail.Actors.Add(actors[i].Trim());
                }
            }
            string[] directors = videoDetail[5].InnerText.Replace("&nbsp", " ").Split(';');
            for (int i = 0; i < directors.Length; i++)
            {
                if (!string.IsNullOrEmpty(directors[i]))
                {
                    movieDetail.Directors.Add(directors[i].Trim());
                }
            }
            movieDetail.Collection = videoDetail[6].InnerText.Trim(new char[] { '\n', '\r', ' '});
            movieDetail.Studio = videoDetail[7].InnerText.Trim(new char[] { '\n', '\r', ' '});
            string[] tags = videoDetail[9].InnerText.Replace("&nbsp", " ").Split(';');
            for (int i = 0; i < tags.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(tags[i]))
                {
                    movieDetail.Tags.Add(tags[i].Trim(new char[] { '\n', '\r',' '}));
                }
            }
            movieDetail.SerialNO = videoDetail[10].InnerText.Trim(new char[] { '\n', '\r',' '});
            //add title
            var title = doc.DocumentNode.SelectSingleNode("//h1[@id = 'title']");
            movieDetail.Title = title.InnerText;
        }
        //get the movie info and fill into an instance of MoviesDetail class
        void GetDVDDetailFromWeb(HtmlAgilityPack.HtmlDocument doc)
        {   //grabbing movie info from web  
            var videoDetail = doc.DocumentNode.SelectNodes("//table[@class = 'mg-b20']/tr/td/following-sibling::*").ToList();
            //add movie info
            movieDetail.ReleaseDate = videoDetail[1].InnerText.Trim(new char[] { '\n', '\r' });
            movieDetail.Length = videoDetail[2].InnerText.Trim(new char[] { '\n', '\r' });
            string[] actors = videoDetail[3].InnerText.Split('\n');
            for (int i = 0; i < actors.Length; i++)
            {
                if (!string.IsNullOrEmpty(actors[i]))
                {
                    movieDetail.Actors.Add(actors[i].Trim());
                }
            }
            string[] directors = videoDetail[4].InnerText.Replace("&nbsp", " ").Split(';');
            for (int i = 0; i < directors.Length; i++)
            {
                if (!string.IsNullOrEmpty(directors[i]))
                {
                    movieDetail.Directors.Add(directors[i].Trim());
                }
            }
            movieDetail.Collection = videoDetail[5].InnerText.Trim(new char[] { '\n', '\r', ' ' });
            movieDetail.Studio = videoDetail[6].InnerText.Trim(new char[] { '\n', '\r', ' ' });
            string[] tags = videoDetail[8].InnerText.Replace("&nbsp", " ").Split(';');
            for (int i = 0; i < tags.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(tags[i]))
                {
                    movieDetail.Tags.Add(tags[i].Trim(new char[] { '\n', '\r', ' ' }));
                }
            }
            movieDetail.SerialNO = videoDetail[9].InnerText.Trim(new char[] { '\n', '\r', ' ' });
            //add title
            var title = doc.DocumentNode.SelectSingleNode("//h1[@id = 'title']");
            movieDetail.Title = title.InnerText;
        }
        //add screenshots
        void GetScreenShots(HtmlAgilityPack.HtmlDocument doc)
        {   //grabbing samples' URL from web
            var samplesNodesList = doc.DocumentNode.SelectNodes("//div[@id = 'sample-image-block']/a/img").ToList();
            for (int i = 0; i < samplesNodesList.Count(); i++)
            {
                var sampleUrl = samplesNodesList[i].Attributes["src"].Value;
                string[] screenShots = sampleUrl.Split('-');
                string screenShotsUrl = screenShots[0] + "jp-" + screenShots[1];
                movieDetail.Samples.Add(DownloadImage(sampleUrl));
                movieDetail.ScreenShots.Add(DownloadImage(screenShotsUrl));
            }        
        }


        private void btnSearch_Click(object sender, EventArgs e)
        {
            String DigitalSerialNo = DealWithDigitalSerialNo(txbSerialNo.Text.Trim().ToLower());
            String DVDSerialNo = DealWithDVDSerialNo(txbSerialNo.Text.Trim().ToLower());
            string DmmDigitalUrl = "http://www.dmm.co.jp/digital/videoa/-/detail/=/cid=" + DigitalSerialNo;
            string DmmDVDUrl = "http://www.dmm.co.jp/mono/dvd/-/detail/=/cid=" + DVDSerialNo;
            
            HtmlWeb DetailHtmlWeb = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument digitalHtmlDocument = DetailHtmlWeb.Load(DmmDigitalUrl);
            HtmlAgilityPack.HtmlDocument dvdHtmlDocument = DetailHtmlWeb.Load(DmmDVDUrl);
            //check if the Serisal NO exists
            if (CheckExists(digitalHtmlDocument) && CheckExists(dvdHtmlDocument))
            {
                MessageBox.Show("Please check the serial NO that you input.");
            }
            //search download version
            else if (digitalHtmlDocument.DocumentNode.SelectSingleNode("//a[@name='package-image']") != null)
            {
                //Get the Url of the poster
                var posterUrl = digitalHtmlDocument.DocumentNode.SelectSingleNode("//a[@name='package-image']").Attributes["href"].Value;
                GetDigitalDetailFromWeb(digitalHtmlDocument);
                if (digitalHtmlDocument.DocumentNode.SelectSingleNode("//div[@id = 'sample-image-block']/a/img") != null)
                { GetScreenShots(digitalHtmlDocument); }
                else { movieDetail.IsContainSamples = false; }
                PopUpPoster(posterUrl);
            }
            //search DVD version
            else if (dvdHtmlDocument.DocumentNode.SelectSingleNode("//a[@name='package-image']") != null)
            {
                //Get the Url of the poster
                var posterUrl = dvdHtmlDocument.DocumentNode.SelectSingleNode("//a[@name='package-image']").Attributes["href"].Value;
                GetDVDDetailFromWeb(dvdHtmlDocument);
                if (dvdHtmlDocument.DocumentNode.SelectSingleNode("//div[@id = 'sample-image-block']/a/img") != null)
                { GetScreenShots(dvdHtmlDocument); }
                else { movieDetail.IsContainSamples = false; }
                PopUpPoster(posterUrl);
            }
        }
    }
    //create a class to store movie info
    class MoviesDetail
    {
        bool isContainSamples = true;
        bool isDVD = false;
        string title = null;
        string releaseDate = null;
        string length = null;
        ArrayList actors = new ArrayList();
        ArrayList directors = new ArrayList();
        string collection = null;
        string studio = null;
        ArrayList tags = new ArrayList();
        string serialNO = null;
        Image poster = null;
        List<Image> samples = new List<Image>();
        List<Image> screenShots = new List<Image>();

        public bool IsDVD { get => isDVD; set => isDVD = value; }
        public string ReleaseDate { get => releaseDate; set => releaseDate = value; }
        public string Length { get => length; set => length = value; }
        public ArrayList Actors { get => actors; set => actors = value; }
        public ArrayList Directors { get => directors; set => directors = value; }
        public string Studio { get => studio; set => studio = value; }
        public ArrayList Tags { get => tags; set => tags = value; }
        public string SerialNO { get => serialNO; set => serialNO = value; }
        public string Collection { get => collection; set => collection = value; }
        public string Title { get => title; set => title = value; }
        public Image Poster { get => poster; set => poster = value; }
        public List<Image> Samples { get => samples; set => samples = value; }
        public List<Image> ScreenShots { get => screenShots; set => screenShots = value; }
        public bool IsContainSamples { get => isContainSamples; set => isContainSamples = value; }
    }
}
