﻿using IptProject.Models.Cafeteria;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;


namespace IptProject.Controllers
{
    public class CafeteriaController : Controller
    {

        static List<FoodItem> globalFooditem = new List<FoodItem>();

        // GET: Cafeteria


        //get user wallet
        //Getavailable items
        //checkout
        //getpendingorderbystudentid
        //vieworder
        //add to cart
        //sessions
        //getitem by id

        static List<FoodItem> globalFooditem = new List<FoodItem>();
        public static List<FoodOrder> lstOrder = new List<FoodOrder>();

        public ActionResult GetProduct()
        {
            globalFooditem.Clear();
            List<FoodItem> lstFoodItems = new List<FoodItem>();
            
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Shared.ServerConfig.GetBaseUrl());
                //HTTP GET
                var responseTask = client.GetAsync("cafeteria/GetItems");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {

                    var readTask = result.Content.ReadAsAsync<FoodItem[]>();
                    readTask.Wait();

                    var fooditems = readTask.Result;

                    foreach (var item in fooditems)
                    {
                        lstFoodItems.Add(item);
                        globalFooditem.Add(item);
                    }
                }
            }

            return View(lstFoodItems);
        }

        public ActionResult fetchImage()
        {
           
            return View();
        }
        public ActionResult GetImage()
        {
            List<FoodItem> lstFoodItems = new List<FoodItem>();
            //FoodItem obj1 = new FoodItem(1, "Tikka", "avc", "Available", 200);
            //FoodItem obj2 = new FoodItem(2, "Pizza", "avc", "Available", 100);
            //lstFoodItems.Add(obj1);
            //lstFoodItems.Add(obj2);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://localhost:44380/api/");
                //HTTP GET
                var responseTask = client.GetAsync("testCafeteria/GetProductWithImage");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {

                    var readTask = result.Content.ReadAsAsync<Dictionary<string, object>>();
                    readTask.Wait();

                    var item = readTask.Result;
                    var base64string = item["base64string"];
                    var contents = Convert.FromBase64String(base64string.ToString());

                    //using (MemoryStream ms = new MemoryStream(contents)) {

                    //    Image returnImage = Image.FromStream(ms);
                    //    ViewBag.Image = returnImage;
                    //} 
                    string mystr = "data:image/jpeg;base64," + base64string.ToString();
                    ViewBag.Image = mystr;

                    //foreach (var item in fooditems)
                    //{
                    //    lstFoodItems.Add(item);
                    //}
                
                }
            }

            return View();
        }
        
     
       
        public List<Cart> GetSessionCart()
        {
            if (Session["SessionCart"] != null)
            {
                return Session["SessionCart"] as List<Cart>;
            }

            return new List<Cart>();

        }
        public void SetSessionCart(Cart obj)
        {
            List<Cart> lst = GetSessionCart();
            int position = -1;
            for(int i = 0; i < lst.Count(); i++)
            {
                if (lst[i].ItemId == obj.ItemId)
                {
                    position = i;
                    
                }
               
            }
            if (position > -1)
            {
                lst[position].Quantity = lst[position].Quantity + obj.Quantity;
                lst[position].SubTotal = obj.SubTotal + lst[position].SubTotal;
                lst[position].TotalAmount = obj.TotalAmount;
            }
            else
            {
                lst.Add(obj);

            }
          
           
            Session["SessionCart"] = lst;
        }
         public ActionResult StarTest()
        {
            return View();
        }
        public ActionResult Checkout(string paymentmethod)
        {


            List < Cart > lstCart = null;//GetSessionCart();
            string StudentId = "1";  //get from session
            int sum = 0;
            foreach (var item in lstCart)
            {
                sum = sum + item.SubTotal;
            }
            int Amount = sum;
            List<Dictionary<string, object>> orderDetails = new List<Dictionary<string, object>>();

            foreach (var item in lstCart)
            {
                Dictionary<string, object> orderdet = new Dictionary<string, object>();
                orderdet["ItemId"] = item.ItemId;
                orderdet["Quantity"] = item.Quantity;

                orderDetails.Add(orderdet);
            }

            Dictionary<string, object> data = new Dictionary<string, object>();
            data["StudentId"] = StudentId;
            data["PaymentMethod"] = paymentmethod;
            data["Amount"] = Amount;
            data["OrderDetail"] = orderDetails;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Shared.ServerConfig.GetBaseUrl());
                //HTTP GET
                var responseTask = client.PostAsJsonAsync("Cafeteria/Checkout", data);

                responseTask.Wait();

                var result = responseTask.Result;
                if (result.StatusCode == HttpStatusCode.Created)
                {

                    Session["SessionCart"] = null;
                    return Content("Success");

                }
                else if (result.StatusCode == HttpStatusCode.Forbidden)
                {
                    return Content("WalletError");

                }
                else
                {
                    return Content("BadRequest");
                }
            }

            // return Content("");
        }

        public ActionResult AddtoCart(int id)
        {
            FoodItem obj = globalFooditem.Where(x => x.ItemId == id).FirstOrDefault();
            return PartialView(obj);
        }
        [HttpPost]
        public ActionResult AddtoCart(Cart obj)
        {

            //getitembyid
            //calculate subtotal
            //add to total
            FoodItem objfood = globalFooditem.Where(x => x.ItemId == obj.ItemId).FirstOrDefault();
            int subtotal = objfood.Price * obj.Quantity;
            obj.SubTotal = subtotal;
            obj.TotalAmount = obj.TotalAmount + subtotal;
            obj.ItemName = objfood.ItemName;
            //obj.StudentId
            obj.Price = objfood.Price;
            //SetSessionCart(obj);
            return Content("Success");
        }
        //public ActionResult CartView()
        //{
        //    return View(GetSessionCart());
        //}
        public ActionResult RemoveItem(int id)
        {
            var lst = Session["SessionCart"] as List<Cart>;
            var item = lst.Where(x => x.ItemId == id).FirstOrDefault();
            lst.Remove(item);
            Session["SessionCart"] = lst;
            return Content("Success");
        }





        //public Volunteer GetSessionUser()
        //{
        //    if (Session[Shared.Constants.SESSION_USER] != null)
        //    {
        //        return Session[Shared.Constants.SESSION_USER] as Volunteer;
        //    }

        //    return null;
        //}

        //public void SetSessionUser(Volunteer obj)
        //{
        //    Session[Shared.Constants.SESSION_USER] = obj;
        //}
        public ActionResult Wallet()
        {
            Wallet obj = new Wallet();
            Dictionary<string, object> data = new Dictionary<string, object>();
            data["StudentID"] = 1;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(Shared.ServerConfig.GetBaseUrl());
                //HTTP GET
                var responseTask = client.PostAsJsonAsync("cafeteria/GetUserWallet", data);

                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {

                    var readTask = result.Content.ReadAsAsync<Wallet>();
                    readTask.Wait();

                    obj = readTask.Result;


                }
            }

            return View(obj);
        }



    }
}

