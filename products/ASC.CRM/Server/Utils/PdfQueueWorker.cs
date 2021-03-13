/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Web;
using ASC.Common.Logging;
using ASC.CRM.Core;
using ASC.Common.Threading.Progress;
using ASC.Common.Web;
using ASC.Core;
using Microsoft.AspNetCore.Http;
using ASC.CRM.Core.Enums;
using log4net;
using ASC.Common;

namespace ASC.Web.CRM.Classes
{  
    [Transient]
    public class PdfQueueWorker
    {
        private readonly ProgressQueue<PdfProgressItem> Queue;
        private readonly int tenantId;
        private readonly Guid userId;

        public PdfQueueWorker(ProgressQueueOptionsManager<PdfProgressItem> progressQueueOptionsManager,
                              PdfProgressItem pdfProgressItem,
                              TenantManager tenantProvider,
                              SecurityContext securityContext)
        {
            Queue = progressQueueOptionsManager.Value;
            PdfProgressItem = pdfProgressItem;
            tenantId = tenantProvider.GetCurrentTenant().TenantId;
            userId = securityContext.CurrentAccount.ID;
        }

        public PdfProgressItem PdfProgressItem { get; }

        public string GetTaskId(int tenantId, int invoiceId)
        {
            return string.Format("{0}_{1}", tenantId, invoiceId);
        }

        public PdfProgressItem GetTaskStatus(int tenantId, int invoiceId)
        {
            var id = GetTaskId(tenantId, invoiceId);

            return Queue.GetStatus(id) as PdfProgressItem;
        }

        public void TerminateTask(int invoiceId)
        {
            var item = GetTaskStatus(tenantId, invoiceId);

            if (item != null)
                Queue.Remove(item);
        }

        public PdfProgressItem StartTask(int invoiceId)
        {
            lock (Queue.SynchRoot)
            {
                var task = GetTaskStatus(tenantId, invoiceId);

                if (task != null && task.IsCompleted)
                {
                    Queue.Remove(task);
                    task = null;
                }

                if (task == null)
                {

                    PdfProgressItem.Configure(GetTaskId(tenantId, invoiceId), tenantId, userId, invoiceId);

                    Queue.Add(PdfProgressItem);
                
                }

                if (!Queue.IsStarted)
                    Queue.Start(x => x.RunJob());

                return task;
            }
        }
    }

    [Transient]
    public class PdfProgressItem : IProgressItem
    {
        private readonly string _contextUrl;
        private int _tenantId;
        private int _invoiceId;
        private Guid _userId;

        public object Id { get; set; }
        public object Status { get; set; }
        public object Error { get; set; }
        public double Percentage { get; set; }
        public bool IsCompleted { get; set; }
        public PdfCreator PdfCreator { get; }
        public SecurityContext SecurityContext { get; }
        public TenantManager TenantManager { get; }

        public PdfProgressItem(IHttpContextAccessor httpContextAccessor)
        {           
            _contextUrl = httpContextAccessor.HttpContext != null ? httpContextAccessor.HttpContext.Request.GetUrlRewriter().ToString() : null;
        
            Status = ProgressStatus.Queued;
            Error = null;
            Percentage = 0;
            IsCompleted = false;
        }

        public void Configure(object id,
                                int tenantId,
                               Guid userId,
                               int invoiceId)
        {
            Id = id;
            _tenantId = tenantId;
            _invoiceId = invoiceId;
            _userId = userId;
        }

        public void RunJob()
        {
            try
            {
                Percentage = 0;
                Status = ProgressStatus.Started;

                TenantManager.SetCurrentTenant(_tenantId);

                SecurityContext.AuthenticateMe(_userId);

                //if (HttpContext.Current == null && !WorkContext.IsMono)
                //{
                //    HttpContext.Current = new HttpContext(
                //        new HttpRequest("hack", _contextUrl, string.Empty),
                //        new HttpResponse(new System.IO.StringWriter()));
                //}

                PdfCreator.CreateAndSaveFile(_invoiceId);

                Percentage = 100;
                Status = ProgressStatus.Done;
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Web").Error(ex);

                Percentage = 0;
                Status = ProgressStatus.Failed;
                Error = ex.Message;
            }
            finally
            {
                // fake httpcontext break configuration manager for mono
                if (!WorkContext.IsMono)
                {
                    //if (HttpContext.Current != null)
                    //{
                    //    new DisposableHttpContext(HttpContext.Current).Dispose();
                    //    HttpContext.Current = null;
                    //}
                }

                IsCompleted = true;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}