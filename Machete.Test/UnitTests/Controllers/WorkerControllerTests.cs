﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Machete.Data;
using Machete.Service;
using Machete.Data.Infrastructure;
using Machete.Web.Controllers;
using System.Web.Mvc;
using Machete.Domain;
using Machete.Test;
using Machete.Web.ViewModel;

namespace Machete.Test.Controllers
{
    [TestClass]
    public class WorkerControllerTests
    {
        Mock<IWorkerService> _wserv;
        Mock<IPersonService> _pserv;
        Mock<IImageService> _iserv;
        //
        //   Testing /Index functionality
        //
        [TestMethod]
        public void WorkerController_index_get_WorkIndexViewModel()
        {
            //Arrange
            _wserv = new Mock<IWorkerService>();
            _pserv = new Mock<IPersonService>();
            _iserv = new Mock<IImageService>();
            var _ctrlr = new WorkerController(_wserv.Object, _pserv.Object, _iserv.Object);
            //Act
            var result = (ViewResult)_ctrlr.Index();
            //Assert
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(WorkerIndex));
        }

        [TestMethod]
        public void WorkerController_create_get_returns_person()
        {
            //Arrange
            _wserv = new Mock<IWorkerService>();
            _pserv = new Mock<IPersonService>();
            _iserv = new Mock<IImageService>();
            var _ctrlr = new WorkerController(_wserv.Object, _pserv.Object, _iserv.Object);
            //Act
            var result = (ViewResult)_ctrlr.Create(0);
            //Assert
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(WorkerViewModel));
        }

        [TestMethod]
        public void WorkerController_create_post_valid_redirects_to_Index()
        {
            //Arrange
            var _worker = new Worker();
            var _person = new Person();
            var _viewmodel = new WorkerViewModel();
            _viewmodel.person = _person;
            _viewmodel.worker = _worker;
            //
            _wserv = new Mock<IWorkerService>();
            _pserv = new Mock<IPersonService>();
            _wserv.Setup(p => p.CreateWorker(_worker, "UnitTest")).Returns(_worker);
            _pserv.Setup(p => p.CreatePerson(_person, "UnitTest")).Returns(_person);
            _iserv = new Mock<IImageService>();
            var _ctrlr = new WorkerController(_wserv.Object, _pserv.Object, _iserv.Object);
            //Act
            var result = (RedirectToRouteResult)_ctrlr.Create(_worker, "UnitTest");
            //Assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [TestMethod]
        public void WorkerController_create_post_invalid_returns_view()
        {
            //Arrange
            var _worker = new Worker();
            var _person = new Person();
            var _viewmodel = new WorkerViewModel();
            _viewmodel.person = _person;
            _viewmodel.worker = _worker;
            //
            _pserv = new Mock<IPersonService>();
            _wserv = new Mock<IWorkerService>();
            _wserv.Setup(p => p.CreateWorker(_worker, "UnitTest")).Returns(_worker);
            _pserv.Setup(p => p.CreatePerson(_person, "UnitTest")).Returns(_person);
            _iserv = new Mock<IImageService>();
            var _ctrlr = new WorkerController(_wserv.Object, _pserv.Object, _iserv.Object);
            _ctrlr.ModelState.AddModelError("TestError", "foo");
            //Act
            var result = (ViewResult)_ctrlr.Create(_worker, "UnitTest");
            //Assert
            var error = result.ViewData.ModelState["TestError"].Errors[0];
            Assert.AreEqual("foo", error.ErrorMessage);
        }

        //
        //   Testing /Edit functionality
        //
        #region edittests
        [TestMethod]
        public void WorkerController_edit_get_returns_worker()
        {
            //Arrange
            var _worker = new Worker();
            var _person = new Person();
            var _viewmodel = new WorkerViewModel();
            _viewmodel.person = _person;
            _viewmodel.worker = _worker;
            //
            _pserv = new Mock<IPersonService>();
            _wserv = new Mock<IWorkerService>();
            int testid = 4242;
            Person fakeperson = new Person();
            _wserv.Setup(p => p.GetWorker(testid)).Returns(_worker);
            _iserv = new Mock<IImageService>();
            var _ctrlr = new WorkerController(_wserv.Object, _pserv.Object, _iserv.Object);
            //Act
            ViewResult result = (ViewResult)_ctrlr.Edit(testid);
            //Assert
            Assert.IsInstanceOfType(result.ViewData.Model, typeof(WorkerViewModel));
        }

        [TestMethod]
        public void WorkerController_edit_post_valid_updates_model_redirects_to_index()
        {
            //Arrange
            _pserv = new Mock<IPersonService>();
            _wserv = new Mock<IWorkerService>();
            int testid = 4242;
            FormCollection fakeform = _fakeCollection(testid);

            Worker fakeworker = new Worker();
            Worker savedworker = new Worker();
            Person fakeperson = new Person();
            fakeworker.Person = fakeperson;
            WorkerViewModel _viewmodel = new WorkerViewModel();
            _viewmodel.person = fakeperson;
            _viewmodel.worker = fakeworker;

            string user = "TestUser";
            _wserv.Setup(p => p.GetWorker(testid)).Returns(fakeworker);
            _pserv.Setup(p => p.GetPerson(testid)).Returns(fakeperson);
            _wserv.Setup(x => x.SaveWorker(It.IsAny<Worker>(),
                                          It.IsAny<string>())
                                         ).Callback((Worker p, string str) =>
                                         {
                                             savedworker = p;
                                             user = str;
                                         });
            _iserv = new Mock<IImageService>();
            var _ctrlr = new WorkerController(_wserv.Object, _pserv.Object, _iserv.Object);
            _ctrlr.SetFakeControllerContext();
            _ctrlr.ValueProvider = fakeform.ToValueProvider();
            //Act
            //TODO Solve TryUpdateModel moq problem
            var result = _ctrlr.Edit(testid, fakeworker, "UnitTest") as RedirectToRouteResult;
            //Assert
            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual(fakeworker, savedworker);
            Assert.AreEqual(savedworker.height, "UnitTest");
            Assert.AreEqual(savedworker.height, "UnitTest");
        }

        private FormCollection _fakeCollection(int id)
        {
            FormCollection _fc = new FormCollection();
            _fc.Add("ID", id.ToString());
            _fc.Add("firstname1", "blah_firstname");
            //_fc.Add("person.firstname2", "");
            _fc.Add("lastname1", "unittest");
            //_fc.Add("person.lastname2", "");
            //_fc.Add("person.address1", "");
            //_fc.Add("person.address2", "");
            //_fc.Add("person.city", "");
            //_fc.Add("person.state", "");
            //_fc.Add("person.zipcode", "");
            //_fc.Add("person.phone", "");
            _fc.Add("gender", "M");
            //_fc.Add("person.genderother", "");          
            _fc.Add("RaceID", "1");     //Every required field must be populated,
            _fc.Add("height", "UnitTest");  //or result will be null.
            _fc.Add("weight", "UnitTest");
            _fc.Add("englishlevelID", "1");
            _fc.Add("dateinUSA", "1/1/2001");
            _fc.Add("dateinseattle", "1/1/2001");
            _fc.Add("maritalstatus", "S");
            _fc.Add("numofchildren", "1");
            _fc.Add("incomeID", "1");
            _fc.Add("dwccardnum", "12345");
            _fc.Add("neighborhoodID", "1");
            _fc.Add("countryoforigin", "USA");
            _fc.Add("memberexpirationdate", "1/1/2002");
            return _fc;
        }
        #endregion  
    }
}