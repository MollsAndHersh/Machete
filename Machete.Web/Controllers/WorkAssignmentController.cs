﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Machete.Data;
using Machete.Data.Infrastructure;
using Machete.Domain;
//using Machete.Helpers;
using Machete.Service;
using Machete.Web.Helpers;
using NLog;
using Machete.Web.ViewModel;
using System.Web.Routing;
using Machete.Web.Models;
using System.Data.Objects;
using System.Data.Objects.SqlClient;

namespace Machete.Web.Controllers
{
    [ElmahHandleError]
    public class WorkAssignmentController : MacheteController
    {
        private readonly IWorkAssignmentService waServ;
        private readonly IWorkerService wkrServ;
        private readonly IWorkOrderService woServ;
        private readonly IWorkerSigninService wsiServ;

        public WorkAssignmentController(IWorkAssignmentService workAssignmentService,
                                        IWorkerService workerService,
                                        IWorkOrderService workOrderService,
                                        IWorkerSigninService signinService)
        {
            this.waServ = workAssignmentService;
            this.wkrServ = workerService;
            this.woServ = workOrderService;
            this.wsiServ = signinService;
        }
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            System.Globalization.CultureInfo CI = (System.Globalization.CultureInfo)Session["Culture"];            
        }

        #region Index
        //
        // GET: /WorkAssignment/
        //
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult Index()
        {
            WorkAssignmentIndex _model = new WorkAssignmentIndex();
            //_model.todaysdate = DateTime.Today.ToShortDateString();
            _model.todaysdate = System.String.Format("{0:dddd, d MMMM yyyy}", DateTime.Today);

            return View(_model);
        }

        #endregion

        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult AjaxHandler(jQueryDataTableParam param)
        {
            //Get all the records
            System.Globalization.CultureInfo CI = (System.Globalization.CultureInfo)Session["Culture"];

            dTableList<WorkAssignment> was = waServ.GetIndexView(new dispatchViewOptions {
                    CI = CI,
                    search = param.sSearch,
                    date = param.todaysdate == null ? null : (DateTime?)DateTime.Parse(param.todaysdate),
                    dwccardnum = Convert.ToInt32(param.dwccardnum),
                    woid = Convert.ToInt32(param.searchColName("WOID")),
                    orderDescending = param.sSortDir_0 == "asc" ? false : true,
                    displayStart = param.iDisplayStart,
                    displayLength = param.iDisplayLength,
                    sortColName=param.sortColName(),
                    wa_grouping = param.wa_grouping,
                    typeofwork_grouping = param.typeofwork_grouping,
                    status = param.status,
                    showPending = param.showPending
            });
            var result = from p in was.query select new { 
                            tabref = _getTabRef(p),
                            tablabel = _getTabLabel(p),
                            WOID = Convert.ToString(p.workOrderID),
                            WAID = Convert.ToString(p.ID),
                            recordid = Convert.ToString(p.ID),
                            pWAID = p.getFullPseudoID(), 
                            englishlevel = Convert.ToString(p.englishLevelID),
                            skill =  LookupCache.byID(p.skillID, CI.TwoLetterISOLanguageName),
                            hourlywage = System.String.Format("${0:f2}", p.hourlyWage),
                            hours = Convert.ToString(p.hours),
                            hourRange = p.hourRange > 0 ? Convert.ToString(p.hourRange) : "",
                            days = Convert.ToString(p.days),
                            description = p.description,
                            dateupdated = Convert.ToString(p.dateupdated), 
                            updatedby = p.Updatedby,
                            dateTimeofWork = p.workOrder.dateTimeofWork.ToString(),
                            status = p.workOrder.status.ToString(),
                            earnings = System.String.Format("${0:f2}",(p.hourlyWage * p.hours * p.days)),
                            WSIID = p.workerSigninID ?? 0,
                            WID = p.workerAssignedID ?? 0,
                            assignedWorker = p.workerAssigned != null ? p.workerAssigned.dwccardnum + " " + p.workerAssigned.Person.fullName() : "",
                            requestedList = p.workOrder.workerRequests.Select(a => a.fullNameAndID).ToArray(),
                            asmtStatus = _getStatus(p)
                };
 
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = was.totalCount,
                iTotalDisplayRecords = was.filteredCount,
                aaData = result
            },
            JsonRequestBehavior.AllowGet);
        }
        //
        // _getStatus
        private string _getStatus(WorkAssignment asmt)
        {
            if (asmt.workerAssignedID > 0 && asmt.workerSigninID > 0) // green
                return "completed";
            if (asmt.workerAssignedID == null && asmt.workOrder.status == WorkOrder.iCompleted)
                return "incomplete";
            if (asmt.workerAssignedID > 0 && asmt.workerSigninID == null && asmt.workOrder.status == WorkOrder.iCompleted)
                return "orphaned";
            if (asmt.workOrder.status == WorkOrder.iCancelled)
                return "cancelled";
            if (asmt.workOrder.status == WorkOrder.iActive) // blue
                return "active";
            return null;
        }
        //
        // _getTabRef
        //
        private string _getTabRef(WorkAssignment wa)
        {
            return "/WorkAssignment/Edit/" + Convert.ToString(wa.ID);
        }
        
        //
        // _getTabLabel
        //
        private string _getTabLabel(WorkAssignment wa)
        {
            return Machete.Web.Resources.WorkAssignments.tabprefix + wa.getFullPseudoID();
        }            
        //
        // GET: /WorkAssignment/Create
        //
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        #region Create
        public ActionResult Create(int WorkOrderID)
        {
            WorkAssignment _assignment = new WorkAssignment();
            _assignment.active = true;
            _assignment.workOrderID = WorkOrderID;
            _assignment.skillID = Lookups.skillDefault;
            _assignment.hours = Lookups.hoursDefault;
            _assignment.days = Lookups.daysDefault;
            _assignment.hourlyWage = Lookups.hourlyWageDefault;
            return PartialView(_assignment);
        }

        //
        // POST: /WorkAssignment/Create
        //
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult Create(WorkAssignment assignment, string userName)
        {
            UpdateModel(assignment);
            assignment.workOrder = woServ.GetWorkOrder(assignment.workOrderID);
            assignment.incrPseudoID();
            //assignment.workOrder.waPseudoIDCounter++;
            //assignment.pseudoID = assignment.workOrder.waPseudoIDCounter;
            WorkAssignment newAssignment = waServ.Create(assignment, userName);

            return Json(new
            {
                sNewRef = _getTabRef(newAssignment),
                sNewLabel = _getTabLabel(newAssignment),
                iNewID = newAssignment.ID
            },
            JsonRequestBehavior.AllowGet);
        }
        #endregion

        
        //
        // POST: /WorkAssignment/Edit/5
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        #region Duplicate
        public ActionResult Duplicate(int id, string userName)
        {
            WorkAssignment _assignment = waServ.Get(id);
            WorkAssignment duplicate = _assignment;
            duplicate.incrPseudoID();
            //duplicate.workOrder.waPseudoIDCounter++;
            //duplicate.pseudoID = duplicate.workOrder.waPseudoIDCounter;
            duplicate.workerAssigned = null;
            duplicate.workerAssignedID = null;
            waServ.Create(duplicate, userName);
            return Json(new
            {
                sNewRef = _getTabRef(duplicate),
                sNewLabel = _getTabLabel(duplicate),
                iNewID = duplicate.ID
            },
            JsonRequestBehavior.AllowGet);

        }
        #endregion

        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager")]
        #region Assign
        public ActionResult Assign(int waid, int wsiid, string userName)
        {
            WorkerSignin signin = wsiServ.Get(wsiid);          
            WorkAssignment assignment = waServ.Get(waid);
            waServ.Assign(assignment, signin, userName);

            return Json(new
            {
                jobSuccess = true
            }, JsonRequestBehavior.AllowGet);            
        }

        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager")]
        public JsonResult Unassign(int? waid, int? wsiid, string userName)
        {
            waServ.Unassign(waid, wsiid, userName);
            return Json(new
            {
                jobSuccess = true,
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        //
        // GET: /WorkAssignment/Edit/5
        //
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        #region Edit
        public ActionResult Edit(int id)
        {
            WorkAssignment wa = waServ.Get(id);
            return PartialView(wa);
        }
        //
        // POST: /WorkAssignment/Edit/5
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public ActionResult Edit(int id, int? workerAssignedID, string userName)
        {
            WorkAssignment asmt = waServ.Get(id);
            //check if workerAssigned changed; if so, Unassign
            int? origWorker = asmt.workerAssignedID;
            if (workerAssignedID != origWorker)
                waServ.Unassign(asmt.ID, asmt.workerSigninID, userName);     
            //Update from HTML attributes
            UpdateModel(asmt);
            //Save will link workerAssigned to Assignment record
            // if changed from orphan assignment
            waServ.Save(asmt, userName);
                
            return Json(new { jobSuccess = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion      
        //
        //GET: /WorkAssignment/View/5
        //
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        #region View
        public ActionResult View(int id)
        {
            WorkAssignment workAssignment = waServ.Get(id);
            
            return View(workAssignment);
        }
        #endregion
        #region Delete
        //
        // POST: /WorkAssignment/Delete/5
        [HttpPost, UserNameFilter]
        [Authorize(Roles = "Administrator, Manager, PhoneDesk")]
        public JsonResult Delete(int id, FormCollection collection, string user)
        {
            waServ.Delete(id, user);

            return Json(new
            {
                status = "OK",
                jobSuccess = true,
                deletedID = id
            },
            JsonRequestBehavior.AllowGet);
        }
        #endregion
    }


}
