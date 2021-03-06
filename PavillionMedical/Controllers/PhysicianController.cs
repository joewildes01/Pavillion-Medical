
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using PavillionData.Models;
using PavillionData.Models.ViewModels;
using PavillionData.Repository;
using System.Data.Entity;
using System.IO;

namespace PavillionMedical.Controllers
{
    public class PhysicianController : Controller
    {
        private readonly PavillionContext Context = new PavillionContext();

        // GET: Physician
        [Authorize]
        public ActionResult Index()
        {
            var UserId = User.Identity.GetUserId();
            var Profile = Context.Physicians.Where(c => c.UserId == UserId).FirstOrDefault();

            if (Profile == null)
            {
                return RedirectToAction("FirstView");
            }

            PhysicianPersonalProfile physicianPersonal = new PhysicianPersonalProfile()
            {             
                physician = Profile,
                ChatCodes = Profile.PatientPins.OrderByDescending(c => c.CodeId).ToList()
            };

            return View(physicianPersonal);
        }
        [Authorize]
        public ActionResult Settings()
        {
            var UserId = User.Identity.GetUserId();
            var Profile = Context.Physicians.Where(c => c.UserId == UserId).FirstOrDefault();

            if (Profile == null)
            {
                return RedirectToAction("FirstView");
            }
            return View(Profile);
        }

        public ActionResult GenerateCode(PatientPin chatCode)
        {
            Random random = new Random();

            var UserId = User.Identity.GetUserId();
            var Profile = Context.Physicians.Where(c => c.UserId == UserId).FirstOrDefault();

            chatCode = new PatientPin()
            {
                CodeContent = random.Next(00000, 99999),
                DateGenerated = DateTime.Now,
                Status = 0,
              
            };

            regenerate:

            var ExistingCodes = Profile.PatientPins.Where(c => c.CodeContent == chatCode.CodeContent).FirstOrDefault();

            if (ExistingCodes != null)
            {
                chatCode.CodeContent = random.Next(00000, 99999);
                goto regenerate;
            }

            Profile.PatientPins.Add(chatCode);
            Context.ChatCodes.Add(chatCode);

            Context.SaveChanges();

            return RedirectToAction("Index");
        }
        [Authorize]
        public ActionResult FirstView()
        {
            return View();
        }
        //Create Physician Profile
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(string UserId, Physician Physician)
        {
            UserId = User.Identity.GetUserId();

            try
            {
                Physician = new Physician
                {
                    UserId = UserId,
                    Title = Physician.Title,
                    FirstName = Physician.FirstName,
                    LastName = Physician.LastName,
                    Practice = Physician.Practice,
                    Specialisation = Physician.Specialisation,
                    Qualifications = Physician.Qualifications,
                    Biography = Physician.Biography,

                };

            }
            catch (Exception)
            {

                return Content("placeholder");
            }

            Context.Physicians.Add(Physician);
            Context.SaveChanges();

            return RedirectToAction("Index");
        }
        //Edit Physician Profile
        [Authorize]
        public ActionResult Edit()
        {
           
            return View();

        }
        [HttpPost]
        public ActionResult Edit(string UserId, Physician Physician)
        {
            UserId = User.Identity.GetUserId();
            var physicianData = Context.Physicians.Where(c => c.UserId == UserId).FirstOrDefault();

            if (physicianData != null)
            {
                try
                {
                    physicianData.FirstName = Physician.FirstName;
                    physicianData.LastName = Physician.LastName;
                    physicianData.Biography = Physician.Biography;
                    physicianData.Title = Physician.Title;
                    physicianData.Qualifications = Physician.Qualifications;
                    physicianData.Specialisation = Physician.Specialisation;
                    physicianData.Practice = Physician.Practice;
                }
                catch (Exception)
                {

                    return Content("Place holder");
                }
                
            }

            if (physicianData.FirstName == null)
            {
                Context.Entry(physicianData).Property(z => z.FirstName).IsModified = false;
            }
            if (physicianData.LastName == null)
            {
                Context.Entry(physicianData).Property(z => z.LastName).IsModified = false;
            }
            if (physicianData.Biography == null)
            {
                Context.Entry(physicianData).Property(z => z.Biography).IsModified = false;
            }
            if (physicianData.Title == null)
            {
                Context.Entry(physicianData).Property(z => z.Title).IsModified = false;
            }
            if (physicianData.Qualifications == null)
            {
                Context.Entry(physicianData).Property(z => z.Qualifications).IsModified = false;
            }
            if (physicianData.Specialisation == null)
            {
                Context.Entry(physicianData).Property(z => z.Specialisation).IsModified = false;
            }
            if (physicianData.Practice == null)
            {
                Context.Entry(physicianData).Property(z => z.Practice).IsModified = false;
            }

            Context.Entry(physicianData).Property(z => z.UserId).IsModified = false;
                      

            Context.SaveChanges();

            return RedirectToAction("Index");

        }
        
        public ActionResult UploadImage(Messages? messages)
        {
            
            return View();
        }
        [HttpPost]
        public ActionResult UploadImage(Physician physician, Image image, string ImageName, string ImageDesc)
        {

            string UserId = User.Identity.GetUserId();

            string extension = Path.GetExtension(physician.UserPicture.FileName);
            string filename = string.Empty;

            filename = UserId + DateTime.Now.ToString("dd-MM-yyyy--HH-mm-ss") + extension;
            string imagePath = Server.MapPath("~/Content/Img/");

            string fullPath = "Content/Img/" + filename;

            Physician physician1 = Context.Physicians.Where(c => c.UserId == UserId).FirstOrDefault();

            try
            {
                physician1.ImagePath = fullPath;
                physician.UserPicture.SaveAs(Path.Combine(imagePath, filename));

                image = new Image
                {
                    Filename = filename,
                    ImagePath = fullPath,
                    ImageName = ImageName,
                    ImageDesc = ImageDesc
                };

                physician1.Images.Add(image);
                Context.Images.Add(image);
                Context.SaveChanges();

                return RedirectToAction("ViewImages", new { messages = Messages.Success });
            }
            catch (Exception)
            {
                return View(new { Message = Messages.Error });
                throw;
            }

            
        }
        public ActionResult ChangeProfilePicture(int id)
        {
            string UserId = User.Identity.GetUserId();
            Physician physician = Context.Physicians.Where(c => c.UserId == UserId).FirstOrDefault();
            Image image = Context.Images.Where(c => c.ImageId == id).FirstOrDefault();

            try
            {
                physician.ImagePath = image.ImagePath;
                Context.SaveChanges();
                return RedirectToAction("ViewImages", new { messages = Messages.PPSuccess });
                
            }
            catch (Exception)
            {

                return RedirectToAction("ViewImages", new { messages = Messages.Error });
            }
            

        }
        public ActionResult ViewImages(Messages? messages) //add album view model
        {
            ViewBag.StatusMessage =
            messages == Messages.Error ? "Something has gone wrong, if the problem persists contact support."
                : messages == Messages.Success ? "Your Image has been successfully uploaded."
                : messages == Messages.PPSuccess ? "Your profile picture has successfully been changed"
                : "";

            var userid = User.Identity.GetUserId();
            var profile = Context.Physicians.Where(c => c.UserId == userid).FirstOrDefault();

            var images = profile.Images.ToList();

            return View(images);
        }
        public ActionResult ViewMessages()
        {
            var userid = User.Identity.GetUserId();
            var profile = Context.Physicians.Where(c => c.UserId == userid).FirstOrDefault();

            var messages = profile.PatientMessages.ToList();

            return View(messages);
        }
        public ActionResult SetAsRead(int id)
        {
            var userid = User.Identity.GetUserId();
            var profile = Context.Physicians.Where(c => c.UserId == userid).FirstOrDefault();

            var messages = profile.PatientMessages.Where(c => c.MessageId == id).FirstOrDefault();

            messages.IsRead = true;

            Context.SaveChanges();

            return RedirectToAction("ViewMessages");
        }
        public ActionResult ImageDetails(int id)
        {
            Image image = Context.Images.Where(c => c.ImageId == id).FirstOrDefault();

            return View(image);
        }
        public enum Messages
        {
            Success,
            Error,
            PPSuccess,

        }
    }
}