using DAL;
using Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Controllers.AccessControl;

namespace Controllers
{
    public class MediasController : Controller
    {
        private IEnumerable<Media> VisibleMedias()
        {
            Models.User connectedUser = Models.User.ConnectedUser;
            if (connectedUser == null)
                return new List<Media>();

            return DB.Medias.ToList().Where(m => m.Shared || m.OwnerId == connectedUser.Id);
        }

        private bool CanAccessMedia(Media media)
        {
            if (media == null)
                return false;

            Models.User connectedUser = Models.User.ConnectedUser;
            if (connectedUser == null)
                return false;

            return media.Shared || media.OwnerId == connectedUser.Id || connectedUser.IsAdmin;
        }

        private bool CanEditMedia(Media media)
        {
            if (media == null)
                return false;

            Models.User connectedUser = Models.User.ConnectedUser;
            if (connectedUser == null)
                return false;

            return media.OwnerId == connectedUser.Id || connectedUser.IsAdmin;
        }

        private List<string> VisibleCategories()
        {
            List<string> categories = new List<string>();

            foreach (Media media in VisibleMedias().OrderBy(m => m.Category))
            {
                if (categories.IndexOf(media.Category) == -1)
                    categories.Add(media.Category);
            }

            return categories;
        }

        private void InitSessionVariables()
        {
            if (Session["CurrentMediaId"] == null) Session["CurrentMediaId"] = 0;
            if (Session["CurrentMediaTitle"] == null) Session["CurrentMediaTitle"] = "";
            if (Session["Search"] == null) Session["Search"] = false;
            if (Session["SearchString"] == null) Session["SearchString"] = "";
            if (Session["SelectedCategory"] == null) Session["SelectedCategory"] = "";
            if (Session["Categories"] == null) Session["Categories"] = VisibleCategories();
            if (Session["SortByTitle"] == null) Session["SortByTitle"] = true;
            if (Session["SortAscending"] == null) Session["SortAscending"] = true;
            ValidateSelectedCategory();
        }

        private void ResetCurrentMediaInfo()
        {
            Session["CurrentMediaId"] = 0;
            Session["CurrentMediaTitle"] = "";
        }

        private void ValidateSelectedCategory()
        {
            if (Session["SelectedCategory"] != null)
            {
                var selectedCategory = (string)Session["SelectedCategory"];
                var medias = VisibleMedias().Where(c => c.Category == selectedCategory);
                if (medias.Count() == 0)
                    Session["SelectedCategory"] = "";
            }
        }

        [UserAccess(Access.View)]
        public ActionResult GetMediasCategoriesList(bool forceRefresh = false)
        {
            try
            {
                InitSessionVariables();

                bool search = (bool)Session["Search"];

                if (search)
                {
                    Session["Categories"] = VisibleCategories();
                    return PartialView();
                }

                return null;
            }
            catch (System.Exception ex)
            {
                return Content("Erreur interne " + ex.Message, "text/html");
            }
        }

        [UserAccess(Access.View)]
        public ActionResult GetMedias(bool forceRefresh = false)
        {
            try
            {
                IEnumerable<Media> result = null;

                if (DB.Medias.HasChanged || forceRefresh)
                {
                    InitSessionVariables();

                    bool search = (bool)Session["Search"];
                    string searchString = ((string)Session["SearchString"]).ToLower();

                    result = VisibleMedias();

                    if (search)
                    {
                        result = result.Where(c => c.Title.ToLower().Contains(searchString));

                        string selectedCategory = (string)Session["SelectedCategory"];
                        if (selectedCategory != "")
                            result = result.Where(c => c.Category == selectedCategory);
                    }

                    if ((bool)Session["SortAscending"])
                    {
                        if ((bool)Session["SortByTitle"])
                            result = result.OrderBy(c => c.Title);
                        else
                            result = result.OrderBy(c => c.PublishDate);
                    }
                    else
                    {
                        if ((bool)Session["SortByTitle"])
                            result = result.OrderByDescending(c => c.Title);
                        else
                            result = result.OrderByDescending(c => c.PublishDate);
                    }

                    Session["Categories"] = VisibleCategories();
                    return PartialView(result);
                }

                return null;
            }
            catch (System.Exception ex)
            {
                return Content("Erreur interne " + ex.Message, "text/html");
            }
        }

        [UserAccess(Access.View)]
        public ActionResult List()
        {
            InitSessionVariables();
            ResetCurrentMediaInfo();
            return View();
        }

        [UserAccess(Access.View)]
        public ActionResult ToggleSearch()
        {
            if (Session["Search"] == null) Session["Search"] = false;
            Session["Search"] = !(bool)Session["Search"];
            return RedirectToAction("List");
        }

        [UserAccess(Access.View)]
        public ActionResult SortByTitle()
        {
            Session["SortByTitle"] = true;
            return RedirectToAction("List");
        }

        [UserAccess(Access.View)]
        public ActionResult ToggleSort()
        {
            Session["SortAscending"] = !(bool)Session["SortAscending"];
            return RedirectToAction("List");
        }

        [UserAccess(Access.View)]
        public ActionResult SortByDate()
        {
            Session["SortByTitle"] = false;
            return RedirectToAction("List");
        }

        [UserAccess(Access.View)]
        public ActionResult SetSearchString(string value)
        {
            Session["SearchString"] = value == null ? "" : value.ToLower();
            return RedirectToAction("List");
        }

        [UserAccess(Access.View)]
        public ActionResult SetSearchCategory(string value)
        {
            Session["SelectedCategory"] = value ?? "";
            return RedirectToAction("List");
        }

        [UserAccess(Access.View)]
        public ActionResult About()
        {
            return View();
        }

        [UserAccess(Access.View)]
        public ActionResult Details(int id)
        {
            Session["CurrentMediaId"] = id;
            Media media = DB.Medias.Get(id);

            if (media != null && CanAccessMedia(media))
            {
                Session["CurrentMediaTitle"] = media.Title;
                return View();
            }

            return RedirectToAction("List");
        }

        [UserAccess(Access.View)]
        public ActionResult GetMediaDetails(bool forceRefresh = false)
        {
            try
            {
                int id = Session["CurrentMediaId"] != null ? (int)Session["CurrentMediaId"] : 0;

                if (id != 0 && (DB.Medias.HasChanged || forceRefresh))
                {
                    Media media = DB.Medias.Get(id);
                    if (media != null && CanAccessMedia(media))
                        return PartialView(media);
                }

                return null;
            }
            catch (System.Exception ex)
            {
                return Content("Erreur interne " + ex.Message, "text/html");
            }
        }

        [UserAccess(Access.Write)]
        public ActionResult Create()
        {
            return View(new Media());
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        [UserAccess(Access.Write)]
        public ActionResult Create(Media media)
        {
            Models.User connectedUser = Models.User.ConnectedUser;
            media.OwnerId = connectedUser.Id;
            DB.Medias.Add(media);
            return RedirectToAction("List");
        }

        [UserAccess(Access.Write)]
        public ActionResult Edit()
        {
            int id = Session["CurrentMediaId"] != null ? (int)Session["CurrentMediaId"] : 0;
            if (id != 0)
            {
                Media media = DB.Medias.Get(id);
                if (media != null && CanEditMedia(media))
                    return View(media);
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        [UserAccess(Access.Write)]
        public ActionResult Edit(Media media)
        {
            int id = Session["CurrentMediaId"] != null ? (int)Session["CurrentMediaId"] : 0;

            Media storedMedia = DB.Medias.Get(id);
            if (storedMedia != null && CanEditMedia(storedMedia))
            {
                media.Id = id;
                media.PublishDate = storedMedia.PublishDate;
                media.OwnerId = storedMedia.OwnerId;
                DB.Medias.Update(media);
                return RedirectToAction("Details/" + id);
            }

            return RedirectToAction("List");
        }

        [UserAccess(Access.Write)]
        public ActionResult Delete()
        {
            int id = Session["CurrentMediaId"] != null ? (int)Session["CurrentMediaId"] : 0;
            if (id != 0)
            {
                Media media = DB.Medias.Get(id);
                if (media != null && CanEditMedia(media))
                    DB.Medias.Delete(id);
            }
            return RedirectToAction("List");
        }

        [UserAccess(Access.Write)]
        public JsonResult CheckConflict(string YoutubeId)
        {
            int id = Session["CurrentMediaId"] != null ? (int)Session["CurrentMediaId"] : 0;

            return Json(
                DB.Medias.ToList().Where(c => c.YoutubeId == YoutubeId && c.Id != id).Any(),
                JsonRequestBehavior.AllowGet
            );
        }
    }
}