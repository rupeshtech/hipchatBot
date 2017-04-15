using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmppBot.GetParsedText
{
    public class Product
    {
        public int Discount
        {
            get
            {
                int num = Convert.ToInt32(((this.NormalpriceDecimal - this.OfferpriceDecimal) * new decimal(100)) / this.NormalpriceDecimal);
                return num;
            }
            set
            {
            }
        }

        public string ImageLink
        {
            get;
            set;
        }

        public decimal NormalpriceDecimal
        {
            get
            {
                decimal num;
                try
                {
                    num = (this.NormlPrice.Contains("-") ? Convert.ToDecimal(this.NormlPrice.Substring(this.NormlPrice.IndexOf("-") + 1).Replace(",", ".")) : Convert.ToDecimal(this.NormlPrice.Replace("euro", "").Replace(",", ".")));
                }
                catch (Exception exception)
                {
                    num = Convert.ToDecimal(this.NormlPrice.Replace("euro", "").Replace("-", "").Replace(",", "."));
                }
                return num;
            }
            set
            {
            }
        }

        public string NormlPrice
        {
            get;
            set;
        }

        public string OfferLink
        {
            get;
            set;
        }

        public string Offerprice
        {
            get;
            set;
        }

        public decimal OfferpriceDecimal
        {
            get
            {
                decimal num;
                try
                {
                    num = (this.Offerprice.Contains("-") ? Convert.ToDecimal(this.Offerprice.Substring(this.Offerprice.IndexOf("-") + 1).Replace(",", ".")) : Convert.ToDecimal(this.Offerprice.Replace("euro", "").Replace(",", ".")));
                }
                catch (Exception exception)
                {
                    num = Convert.ToDecimal(this.Offerprice.Replace("euro", "").Replace("-", "").Replace(",", "."));
                }
                return num;
            }
            set
            {
            }
        }

        public string SuperMarket
        {
            get;
            set;
        }

        public string Text
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public Product()
        {
        }
    }
}
