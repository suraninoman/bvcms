﻿using System;
using System.Linq;
using System.Data.Linq;
using System.IO;
using Drawing = System.Drawing;
using System.Drawing.Imaging;
using UtilityExtensions;

namespace ImageData
{
    public partial class Image
    {
        public static Image NewImageFromBits(byte[] bits, int w, int h)
        {
            var i = new Image();
            i.LoadResizeFromBits(bits, w, h);
            DbUtil.Db.Images.InsertOnSubmit(i);
            DbUtil.Db.SubmitChanges();
            return i;
        }
        private void LoadResizeFromBits(byte[] bits, int w, int h)
        {
            var istream = new MemoryStream(bits);
            var img1 = Drawing.Image.FromStream(istream);
            var ratio = Math.Min(w / (double)img1.Width, h / (double)img1.Height);
            if (ratio >= 1) // image is smaller than requested
                ratio = 1; // same size
            w = Convert.ToInt32(ratio * img1.Width);
            h = Convert.ToInt32(ratio * img1.Height);
            var img2 = new Drawing.Bitmap(img1, w, h);
            var ostream = new MemoryStream();
            img2.Save(ostream, ImageFormat.Jpeg);
            Bits = ostream.GetBuffer();
            Length = Bits.Length;
            img1.Dispose();
            img2.Dispose();
            istream.Close();
            ostream.Close();
        }
        public static Image NewTextFromString(string s)
        {
            var i = new Image();
            i.Mimetype = "text/plain";
            i.Bits = System.Text.Encoding.ASCII.GetBytes(s);
            i.Length = i.Bits.Length;
            DbUtil.Db.Images.InsertOnSubmit(i);
            DbUtil.Db.SubmitChanges();
            return i;
        }
        public static Image NewTextFromBits(byte[] bits)
        {
            var i = new Image();
            i.Mimetype = "text/plain";
            i.Bits = bits;
            i.Length = i.Bits.Length;
            DbUtil.Db.Images.InsertOnSubmit(i);
            DbUtil.Db.SubmitChanges();
            return i;
        }
        public static Image NewImageFromBits(byte[] bits)
        {
            var i = new Image();
            i.LoadImageFromBits(bits);
            DbUtil.Db.Images.InsertOnSubmit(i);
            DbUtil.Db.SubmitChanges();
            return i;
        }
        private void LoadImageFromBits(byte[] bits)
        {
            var istream = new MemoryStream(bits);
            var img1 = Drawing.Image.FromStream(istream);
            var img2 = new Drawing.Bitmap(img1, img1.Width, img1.Height);
            var ostream = new MemoryStream();
            img2.Save(ostream, ImageFormat.Jpeg);
            Mimetype = "image/jpeg";
            Bits = ostream.GetBuffer();
            Length = Bits.Length;
            img1.Dispose();
            img2.Dispose();
            istream.Close();
            ostream.Close();
        }
        public static Image NewImageFromBits(byte[] bits, string type)
        {
            var i = new Image();
            i.LoadFromBits(bits, type);
            DbUtil.Db.Images.InsertOnSubmit(i);
            DbUtil.Db.SubmitChanges();
            return i;
        }
        private void LoadFromBits(byte[] bits, string type)
        {
            Bits = bits;
            Length = Bits.Length;
            Mimetype = type;
        }
        public static void DeleteOnSubmit(int? imageid)
        {
            var i = DbUtil.Db.Images.SingleOrDefault(img => img.Id == imageid);
            if (i == null)
                return;
            DbUtil.Db.Images.DeleteOnSubmit(i);
        }
        public bool HasMedical() // special function
        {
            var line = Medical();
            if (!line.HasValue())
                return false;
            var tt = line.Split(':');
            if (tt.Length == 1)
                return false;
            if (tt[1].ToLower().Contains("none"))
                return false;
            if (tt[1].ToLower().Contains("n/a"))
                return false;
            if (tt[1].ToLower().Contains("nka"))
                return false;
            return tt[1].HasValue();
        }
        public string Medical() // special function
        {
            if (Mimetype != "text/plain")
                return null;
            var t = System.Text.ASCIIEncoding.ASCII.GetString(Bits);
            var q = from li in t.SplitStr("\r\n")
                    where li.StartsWith("Medical:")
                    select li;
            if (q.Count() == 0)
                return null;
            return q.First().Trim();
        }
    }
}
