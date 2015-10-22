using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace StatsDHelper.WebApi.Tests.Integration
{
    public class FakeActionDescriptor : HttpActionDescriptor
    {
        private string _actionName = "ActionName";

        public FakeActionDescriptor()
            : base(new HttpControllerDescriptor(new HttpConfiguration(), "ControllerName", typeof(object)))
        {
        }

        public override Collection<HttpParameterDescriptor> GetParameters()
        {
            throw new NotImplementedException();
        }

        public override Task<object> ExecuteAsync(HttpControllerContext controllerContext, IDictionary<string, object> arguments, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override string ActionName
        {
            get { return _actionName; }
        }

        public void SetActionName(string actionName)
        {
            _actionName = actionName;
        }

        public override Type ReturnType
        {
            get { throw new NotImplementedException(); }
        }
    }
}