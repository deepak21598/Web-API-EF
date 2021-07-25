using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EFWebApi.Models;
using EFWebApi.DataBaseConnection;
using System.Data;

namespace EFWebApi.Controllers
{
    public class ProjectController : ApiController
    {
        db_a76ba2_remindme12Entities db = new db_a76ba2_remindme12Entities();
        DataBase obj = new DataBase();

        [Route("api/GetProject")]
        [HttpGet]
        public IHttpActionResult GetProject(int pagenumber, int pagesize)
        {
            try
            {
                int skip = (pagenumber - 1) * pagesize;
                int take = pagesize;

                var query = from pm in db.PROJECTMASTERs
                            join epl in db.PROJECTEMPLOYEELINKINGs on pm.ID equals epl.PROJECTID
                            join em in db.EMPLOYEEMASTERs on epl.EMPLOYEEID equals em.EMP_ID
                            join emp in db.EMPLOYEEMASTERs on pm.PROJECTMGR equals emp.EMP_ID
                            join dm in db.DEPARTMENTMASTERs on em.DEPARTMENTID equals dm.DEPT_ID
                            orderby pm.NAME ascending
                            select new
                            {
                                ProjectName = pm.NAME,
                                ProjectStartDate = pm.STARTDATE,
                                ProjectEndDate = pm.ENDATE,
                                ProjectManagerName = emp.NAME,
                                ProjectManagerEmail = emp.EMAIL,
                                EmployeeName = em.NAME,
                                EmployeeEmail = em.EMAIL,
                                EmployeeDOJ = em.DOJ,
                                EmployeeDepartment = dm.NAME
                            };

                var data = query.Skip(skip).Take(pagesize);

                int totalCount = query.Count();
                int TotalPages = (int)Math.Ceiling(totalCount / (double)pagesize);

                var response = new
                {
                    result = new
                    {
                        data = data,
                        page = pagenumber,
                        pageSize = pagesize,
                        hasMore = pagenumber < TotalPages ? "Yes" : "No",
                        totalItems = totalCount
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("api/projects")]
        [HttpGet]
        public HttpResponseMessage Projects(int pagenumber, int pagesize)
        {
            try
            {
                string queryCount = "EXEC USP_GETPROJECTS 0,0,1";
                DataTable dtCount = obj.GetData(queryCount);
                int count = Convert.ToInt32(dtCount.Rows[0][0]);

                int skip = (pagenumber - 1) * pagesize;
                int take = pagesize;

                int TotalPages = (int)Math.Ceiling(count / (double)pagesize);

                string query = "EXEC USP_GETPROJECTS " + skip + "," + take + "";
                DataTable dt = obj.GetData(query);
                var response = new
                {
                    result = new
                    {
                        data = dt,
                        page = pagenumber,
                        pageSize = pagesize,
                        hasMore = pagenumber < TotalPages ? "Yes" : "No",
                        totalItems = count
                    }
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        //[Route("api/GetEmployeeEf")]
        [HttpGet]
        public IHttpActionResult GetEmployeeEf() 
        {
            return Ok(db.EMPLOYEEMASTERs.ToList());
        }
        //[Route("api/GetEmployee")]
        [HttpGet]
        public IHttpActionResult GetEmployee()
        {
            DataTable dt = obj.GetData("select * from EMPLOYEEMASTER");
            return Ok(dt);    
        }
    }
}
