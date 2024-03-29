using dotnet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class ReceptionistController : ControllerBase
    {
        private readonly Context _db;

        public ReceptionistController(Context context)
        {
            _db = context;
        }

        [HttpGet("get")]
        public async Task<Response<List<Receptionist>>> GetItems()
        {
            try
            {
                List<Receptionist> receptionistList = await _db.Receptionists.Include(x => x.User).Include(x => x.User.Qualifications).ToListAsync();
                if (receptionistList != null)
                {
                    if (receptionistList.Count > 0)
                    {
                        return new Response<List<Receptionist>>(true, "Success: Acquired data.", receptionistList);
                    }
                }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                return new Response<List<Receptionist>>(false, "Failure: Data does not exist.", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
            catch (Exception exception)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                return new Response<List<Receptionist>>(false, $"Server Failure: Unable to get data. Because {exception.Message}", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }

        [HttpGet("get/id/{id}")]
        public async Task<Response<Receptionist>> GetItemById(int id)
        {
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                Receptionist receptionist = await _db.Receptionists.Include(x => x.User).Include(x => x.User.Qualifications).FirstOrDefaultAsync(x => x.Id == id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (receptionist != null)
                {
                    return new Response<Receptionist>(true, "Success: Acquired data.", receptionist);
                }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                return new Response<Receptionist>(false, "Failure: Data doesnot exist.", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
            catch (Exception exception)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                return new Response<Receptionist>(false, $"Server Failure: Unable to get object. Because {exception.Message}", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }

        [HttpGet("search/{search}")]
        public async Task<Response<List<Receptionist>>> SearchItems(String search)
        {
            try
            {
                if (String.IsNullOrEmpty(search))
                {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    return new Response<List<Receptionist>>(false, "Failure: Enter a valid search string.", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
                List<Receptionist> receptionistList = await _db.Receptionists.Where(x => x.Id.ToString().Contains(search) || x.UserId.ToString().Contains(search) ||
                x.JobType.Contains(search) || x.ShiftTime.Contains(search) || x.User.MaritalStatus.Contains(search) || x.User.Religion.Contains(search) ||
                x.User.FirstName.Contains(search) || x.User.LastName.Contains(search) || x.User.FatherHusbandName.Contains(search) ||
                x.User.Gender.Contains(search) || x.User.Cnic.Contains(search) || x.User.Contact.Contains(search) || x.User.EmergencyContact.Contains(search) ||
                x.User.Email.Contains(search) || x.User.Address.Contains(search) || x.User.Experience.Contains(search) ||
                x.User.FloorNo.ToString().Contains(search)).OrderBy(x => x.Id).Take(10).Include(x => x.User).ToListAsync();
                if (receptionistList != null)
                {
                    if (receptionistList.Count > 0)
                    {
                        return new Response<List<Receptionist>>(true, "Success: Acquired data.", receptionistList);
                    }
                }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                return new Response<List<Receptionist>>(false, "Failure: Database is empty.", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
            catch (Exception exception)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                return new Response<List<Receptionist>>(false, $"Server Failure: Unable to get data. Because {exception.Message}", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }

        [HttpPost("insert")]
        public async Task<Response<Receptionist>> InsertItem(ReceptionistRequest receptionistRequest)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                User user = new User();
                user.UserType = receptionistRequest.UserType;
                user.FirstName = receptionistRequest.FirstName;
                user.LastName = receptionistRequest.LastName;
                user.FatherHusbandName = receptionistRequest.FatherHusbandName;
                user.Gender = receptionistRequest.Gender;
                user.Cnic = receptionistRequest.Cnic;
                user.Contact = receptionistRequest.Contact;
                user.EmergencyContact = receptionistRequest.EmergencyContact;
                user.Email = receptionistRequest.Email;
                user.Address = receptionistRequest.Address;
                user.JoiningDate = receptionistRequest.JoiningDate;
                user.FloorNo = receptionistRequest.FloorNo;
                user.Experience = receptionistRequest.Experience;
                user.DateOfBirth = receptionistRequest.DateOfBirth;
                user.MaritalStatus = receptionistRequest.MaritalStatus;
                user.Religion = receptionistRequest.Religion;
                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();

                if (receptionistRequest.QualificationList != null)
                {
                    if (receptionistRequest.QualificationList.Count > 0)
                    {
                        foreach (QualificationRequest drQualification in receptionistRequest.QualificationList)
                        {
                            Qualification qualification = new Qualification();
                            qualification.UserId = user.Id;
                            qualification.Certificate = drQualification.Certificate;
                            qualification.Description = drQualification.Description;
                            qualification.QualificationType = drQualification.QualificationType;
                            await _db.Qualifications.AddAsync(qualification);
                            await _db.SaveChangesAsync();
                        }
                    }
                }

                Receptionist receptionist = new Receptionist();
                receptionist.UserId = user.Id;
                receptionist.JobType = receptionistRequest.JobType;
                receptionist.ShiftTime = receptionistRequest.ShiftTime;
                await _db.Receptionists.AddAsync(receptionist);
                await _db.SaveChangesAsync();

                transaction.Commit();
                return new Response<Receptionist>(true, "Success: Created object.", receptionist);
            }
            catch (Exception exception)
            {
                transaction.Rollback();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                return new Response<Receptionist>(false, $"Server Failure: Unable to insert object. Because {exception.Message}", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }

        [HttpPut("update/{id}")]
        public async Task<Response<Receptionist>> UpdateItem(int id, ReceptionistRequest receptionistRequest)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                if (id != receptionistRequest.Id)
                {
                    transaction.Rollback();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    return new Response<Receptionist>(false, "Failure: Id sent in body does not match object Id", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                Receptionist receptionist = await _db.Receptionists.Include(x => x.User.Qualifications).FirstOrDefaultAsync(x => x.Id == id); ;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (receptionist == null)
                {
                    transaction.Rollback();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    return new Response<Receptionist>(false, $"Failure: Unable to update receptionist {receptionistRequest.FirstName}. Because Id is invalid. ", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
                receptionist.JobType = receptionistRequest.JobType;
                receptionist.ShiftTime = receptionistRequest.ShiftTime;
                await _db.SaveChangesAsync();

                if (receptionistRequest.QualificationList != null)
                {
                    if (receptionistRequest.QualificationList.Count > 0)
                    {
                        foreach (QualificationRequest drQualification in receptionistRequest.QualificationList)
                        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                            Qualification qualification = await _db.Qualifications.FirstOrDefaultAsync(x => x.Id == drQualification.Id && x.UserId == receptionist.UserId);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                            if (qualification == null)
                            {
                                transaction.Rollback();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                                return new Response<Receptionist>(false, $"Failure: Unable to update qualification {drQualification.Certificate}. Because Id is invalid. ", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            }
                            qualification.Certificate = drQualification.Certificate;
                            qualification.Description = drQualification.Description;
                            qualification.QualificationType = drQualification.QualificationType;
                            await _db.SaveChangesAsync();
                        }
                    }
                }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                User user = await _db.Users.FirstOrDefaultAsync(x => x.Id == receptionist.UserId);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (user == null)
                {
                    transaction.Rollback();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    return new Response<Receptionist>(false, "Failure: Data does not exist.", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
                user.UserType = receptionistRequest.UserType;
                user.FirstName = receptionistRequest.FirstName;
                user.LastName = receptionistRequest.LastName;
                user.FatherHusbandName = receptionistRequest.FatherHusbandName;
                user.Gender = receptionistRequest.Gender;
                user.Cnic = receptionistRequest.Cnic;
                user.Contact = receptionistRequest.Contact;
                user.EmergencyContact = receptionistRequest.EmergencyContact;
                user.Email = receptionistRequest.Email;
                user.Address = receptionistRequest.Address;
                user.JoiningDate = receptionistRequest.JoiningDate;
                user.FloorNo = receptionistRequest.FloorNo;
                user.Experience = receptionistRequest.Experience;
                user.DateOfBirth = receptionistRequest.DateOfBirth;
                user.MaritalStatus = receptionistRequest.MaritalStatus;
                user.Religion = receptionistRequest.Religion;
                await _db.SaveChangesAsync();

                transaction.Commit();
                return new Response<Receptionist>(true, "Success: Updated object.", receptionist);
            }
            catch (Exception exception)
            {
                transaction.Rollback();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                return new Response<Receptionist>(false, $"Server Failure: Unable to update object. Because {exception.Message}", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<Response<Receptionist>> DeleteItemById(int id)
        {
            try
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                Receptionist receptionist = await _db.Receptionists.FirstOrDefaultAsync(x => x.Id == id);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (receptionist == null)
                {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    return new Response<Receptionist>(false, $"Failure: Object with id={id} does not exist.", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                User user = await _db.Users.FindAsync(receptionist.UserId);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                if (user == null)
                {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    return new Response<Receptionist>(false, $"Failure: Object with id={id} does not exist.", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();

                return new Response<Receptionist>(true, "Success: Deleted data.", receptionist);
            }
            catch (Exception exception)
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                return new Response<Receptionist>(false, $"Server Failure: Unable to delete object. Because {exception.Message}", null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }
        }
    }
}
