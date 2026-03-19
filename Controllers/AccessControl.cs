using Models;
using System.Web;
using System.Web.Mvc;

namespace Controllers
{
    public class AccessControl
    {
        public class UserAccess : AuthorizeAttribute
        {
            private Access RequiredAccess { get; set; }

            public UserAccess(Access access = Access.Anonymous) : base()
            {
                RequiredAccess = access;
            }

            protected override bool AuthorizeCore(HttpContextBase httpContext)
            {
                try
                {
                    bool ajaxRequest = httpContext.Request.IsAjaxRequest();

                    if (User.ConnectedUser == null)
                    {
                        if (ajaxRequest)
                        {
                            httpContext.Response.StatusCode = 401;
                            httpContext.Response.End();
                        }
                        else
                        {
                            httpContext.Response.Redirect("/Accounts/Login");
                        }
                        return false;
                    }

                    if (User.ConnectedUser.Access < RequiredAccess || User.ConnectedUser.Blocked)
                    {
                        if (ajaxRequest)
                        {
                            httpContext.Response.StatusCode = 401;
                            httpContext.Response.End();
                        }
                        else
                        {
                            httpContext.Response.Redirect("/Accounts/Login");
                        }
                        return false;
                    }

                    return true;
                }
                catch
                {
                    httpContext.Response.StatusCode = 401;
                    return false;
                }
            }
        }
    }
}