﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Machete.Domain;
using Machete.Data;
using Machete.Data.Infrastructure;

namespace Machete.Service
{
    public interface IWorkerSigninService
    {
        IEnumerable<WorkerSignin> GetWorkerSignins();
        WorkerSignin GetWorkerSignin(int id);
        void CreateWorkerSignin(WorkerSignin workerSignin, string user);
        void DeleteWorkerSignin(int id);
        void SaveWorkerSignin();
        IEnumerable<WorkerSigninView> getView(DateTime date);
        Image getImage(int dwccardnum);
        DateTime getExpireDate(int dwccardnum);
    }
    public class WorkerSigninService : IWorkerSigninService
    {
        private readonly IWorkerSigninRepository signinRepo;
        private readonly IWorkerRepository workerRepo;
        private readonly IPersonRepository personRepo;
        private readonly IUnitOfWork unitOfWork;
        private readonly IImageRepository imageRepo;
        //
        //
        public WorkerSigninService(IWorkerSigninRepository workerSigninRepository, 
                                   IWorkerRepository workerRepository,
                                   IPersonRepository personRepository,
                                   IImageRepository imageRepository,
                                   IUnitOfWork unitOfWork)
        {
            this.signinRepo = workerSigninRepository;
            this.workerRepo = workerRepository;
            this.personRepo = personRepository;
            this.unitOfWork = unitOfWork;
            this.imageRepo = imageRepository;
        }
        #region IWorkerSigninService Members
        //TODO: GetWorkerSignins
        public IEnumerable<WorkerSignin> GetWorkerSignins()
        {
            var signins = signinRepo.GetAll();
            return signins;
        }
        //TODO: GetWorkerSignin
        public WorkerSignin GetWorkerSignin(int id)
        {
            var workerSignin = signinRepo.GetById(id);
            return workerSignin;
        }

        public void CreateWorkerSignin(WorkerSignin signin, string user)
        {
            //Search for worker with matching card number
            Worker _wfound;
            _wfound = workerRepo.GetAllQ().FirstOrDefault(s => s.dwccardnum == signin.dwccardnum);
            if (_wfound != null)
            {
                signin.WorkerID = _wfound.ID;
            }
            //Search for duplicate signin for the same day
            int _sfound = 0;;
            _sfound = signinRepo.GetAllQ().Count(s => s.dateforsignin == signin.dateforsignin &&
                                                     s.dwccardnum == signin.dwccardnum);
            if (_sfound == 0)
            {
                signin.createdby(user);
                signinRepo.Add(signin);
                unitOfWork.Commit();
            }
        }
        //TODO: UnitTest DeleteWorkerSignin
        public void DeleteWorkerSignin(int id)
        {
            var workerSignin = signinRepo.GetById(id);
            signinRepo.Delete(workerSignin);
            unitOfWork.Commit();
        }
        //TODO: UnitTest SaveWorkerSignin
        public void SaveWorkerSignin()
        {
            unitOfWork.Commit();
        }
        //TODO: UnitTest getView
        public IEnumerable<WorkerSigninView> getView(DateTime date)
        {
            //var s_to_w_query = from s in signinRepo.GetAll()
            //                   where s.dateforsignin == date
            //                   join w in workerRepo.GetAll() on s.dwccardnum equals w.dwccardnum into outer
            //                   from row in outer.DefaultIfEmpty(new Worker { ID = 0 })
            //                   join p in personRepo.GetAll() on row.ID equals p.ID into final
            //                   from finalrow in final.DefaultIfEmpty(new Person { ID = 0 })
            //                   orderby s.datecreated descending
            //                   select new WorkerSigninView(finalrow, s);
            //return s_to_w_query;
            var signins = signinRepo.GetAllQ();
            var workers = workerRepo.GetAllQ();
            var persons = personRepo.GetAllQ();

            Worker blank = new Worker { ID = 0 };
            var query = signins.Where(s => s.dateforsignin == date)
                               .Join(workers, s => s.dwccardnum, w => w.dwccardnum, (s, w) => new { s, w })
                               .DefaultIfEmpty()
                               .Join(persons, oj => oj.w.ID, p => p.ID, (oj, p) => new { oj, p }).DefaultIfEmpty()
                               .Select(a => new WorkerSigninView
                               {
                                   dateforsignin = a.oj.s == null ? DateTime.MinValue : a.oj.s.dateforsignin,
                                   dwccardnum = a.oj.s == null ? 0 : a.oj.s.dwccardnum,
                                   signinID = a.oj.s == null ? 0 : a.oj.s.ID,
                                   firstname1 = a.p == null ? null : a.p.firstname1,
                                   firstname2 = a.p == null ? null : a.p.firstname2,
                                   lastname1 = a.p == null ? null : a.p.lastname1,
                                   lastname2 = a.p == null ? null : a.p.lastname2
                               }).AsEnumerable();
            return query;

        }
        //TODO: UnitTest getImage
        public Image getImage(int cardrequest)
        { 
            Worker w_query = workerRepo.GetAllQ().Where(w => w.dwccardnum == cardrequest).AsEnumerable().FirstOrDefault();
            if (w_query == null) return null;
            if (w_query.ImageID != null)
            {
                return imageRepo.Get(i => i.ID == w_query.ImageID);
            }
            return null;
        }
        #endregion
        //TODO: UnitTest 
        public DateTime getExpireDate(int cardrequest)
        {
           //IEnumerable<Worker>  w_query = workerRepo.GetManyQ(w => w.dwccardnum == cardrequest).AsEnumerable();
            var workers = workerRepo.GetAllQ();
            Worker w_query = workers.Where(w => w.dwccardnum == cardrequest).AsEnumerable().FirstOrDefault();
            if (w_query == null)
            {
                //TODO: can't return null for datetime; better way to handle 'no record'?
                return DateTime.MinValue;
            }
            return w_query.memberexpirationdate;
        }
    }
}
