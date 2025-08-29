using HealthCareApp.Models;
using System.Text;

namespace HealthCareApp.Services
{
    public interface IPdfService
    {
        string GenerateAppointmentReportHtml(AppointmentReportData data);
        string GeneratePaymentReportHtml(PaymentReportData data);
        string GeneratePatientReportHtml(PatientReportData data);
    }

    public class PdfService : IPdfService
    {
        public string GenerateAppointmentReportHtml(AppointmentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<title>Appointment Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #4e73df; border-bottom: 2px solid #4e73df; padding-bottom: 10px; }");
            html.AppendLine("h2 { color: #333; margin-top: 30px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            html.AppendLine(".summary { background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }");
            html.AppendLine(".summary-item { display: inline-block; margin: 10px 20px 10px 0; }");
            html.AppendLine(".summary-label { font-weight: bold; color: #333; }");
            html.AppendLine(".summary-value { color: #4e73df; font-size: 18px; font-weight: bold; }");
            html.AppendLine(".cancelled { color: #e74a3b; }");
            html.AppendLine(".completed { color: #1cc88a; }");
            html.AppendLine(".pending { color: #f6c23e; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Header
            html.AppendLine("<h1>Appointment Report</h1>");
            html.AppendLine($"<p><strong>Report Period:</strong> {data.FromDate:MMMM dd, yyyy} to {data.ToDate:MMMM dd, yyyy}</p>");
            html.AppendLine($"<p><strong>Generated On:</strong> {DateTime.Now:MMMM dd, yyyy hh:mm tt}</p>");

            // Summary
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>Summary Statistics</h2>");
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Total Appointments:</div>");
            html.AppendLine($"<div class='summary-value'>{data.TotalAppointments}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Completed:</div>");
            html.AppendLine($"<div class='summary-value completed'>{data.CompletedAppointments}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Pending:</div>");
            html.AppendLine($"<div class='summary-value pending'>{data.PendingAppointments}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Cancelled:</div>");
            html.AppendLine($"<div class='summary-value cancelled'>{data.CancelledAppointments}</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            // Appointments by Specialty
            if (data.AppointmentsBySpecialty.Any())
            {
                html.AppendLine("<h2>Appointments by Specialty</h2>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Specialty</th><th>Number of Appointments</th></tr>");
                foreach (var specialty in data.AppointmentsBySpecialty)
                {
                    html.AppendLine($"<tr><td>{specialty.SpecialtyName}</td><td>{specialty.Count}</td></tr>");
                }
                html.AppendLine("</table>");
            }

            // Detailed Appointments
            if (data.AppointmentDetails.Any())
            {
                html.AppendLine("<h2>Appointment Details</h2>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>ID</th><th>Patient</th><th>Doctor</th><th>Specialty</th><th>Date</th><th>Time</th><th>Status</th></tr>");
                foreach (var appointment in data.AppointmentDetails)
                {
                    var statusClass = appointment.IsCancelled ? "cancelled" : 
                                     appointment.Status == "Completed" ? "completed" : "pending";
                    var status = appointment.IsCancelled ? "Cancelled" : appointment.Status;
                    
                    html.AppendLine($"<tr>");
                    html.AppendLine($"<td>{appointment.AppointmentID}</td>");
                    html.AppendLine($"<td>{appointment.PatientName}</td>");
                    html.AppendLine($"<td>{appointment.DoctorName}</td>");
                    html.AppendLine($"<td>{appointment.SpecialtyName}</td>");
                    html.AppendLine($"<td>{appointment.AppointmentDate:MMM dd, yyyy}</td>");
                    html.AppendLine($"<td>{appointment.AppointmentTime}</td>");
                    html.AppendLine($"<td class='{statusClass}'>{status}</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</table>");
            }

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        public string GeneratePaymentReportHtml(PaymentReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<title>Payment Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #1cc88a; border-bottom: 2px solid #1cc88a; padding-bottom: 10px; }");
            html.AppendLine("h2 { color: #333; margin-top: 30px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            html.AppendLine(".summary { background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }");
            html.AppendLine(".summary-item { display: inline-block; margin: 10px 20px 10px 0; }");
            html.AppendLine(".summary-label { font-weight: bold; color: #333; }");
            html.AppendLine(".summary-value { color: #1cc88a; font-size: 18px; font-weight: bold; }");
            html.AppendLine(".amount { text-align: right; font-weight: bold; }");
            html.AppendLine(".completed { color: #1cc88a; }");
            html.AppendLine(".pending { color: #f6c23e; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Header
            html.AppendLine("<h1>Payment Report</h1>");
            html.AppendLine($"<p><strong>Report Period:</strong> {data.FromDate:MMMM dd, yyyy} to {data.ToDate:MMMM dd, yyyy}</p>");
            html.AppendLine($"<p><strong>Generated On:</strong> {DateTime.Now:MMMM dd, yyyy hh:mm tt}</p>");

            // Summary
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>Summary Statistics</h2>");
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Total Transactions:</div>");
            html.AppendLine($"<div class='summary-value'>{data.TotalTransactions}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Total Revenue:</div>");
            html.AppendLine($"<div class='summary-value'>${data.TotalRevenue:N2}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Completed Revenue:</div>");
            html.AppendLine($"<div class='summary-value completed'>${data.CompletedRevenue:N2}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Pending Revenue:</div>");
            html.AppendLine($"<div class='summary-value pending'>${data.PendingRevenue:N2}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Average Transaction:</div>");
            html.AppendLine($"<div class='summary-value'>${data.AverageTransactionValue:N2}</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            // Detailed Payments
            if (data.PaymentDetails.Any())
            {
                html.AppendLine("<h2>Payment Details</h2>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Payment ID</th><th>Patient</th><th>Booking Ref</th><th>Amount</th><th>Status</th><th>Date</th></tr>");
                foreach (var payment in data.PaymentDetails)
                {
                    var statusClass = payment.PaymentStatus == "Completed" ? "completed" : "pending";
                    
                    html.AppendLine($"<tr>");
                    html.AppendLine($"<td>{payment.PaymentID}</td>");
                    html.AppendLine($"<td>{payment.PatientName}</td>");
                    html.AppendLine($"<td>{payment.BookingRef}</td>");
                    html.AppendLine($"<td class='amount'>${payment.Amount:N2}</td>");
                    html.AppendLine($"<td class='{statusClass}'>{payment.PaymentStatus}</td>");
                    html.AppendLine($"<td>{payment.PaymentDate:MMM dd, yyyy}</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</table>");
            }

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }

        public string GeneratePatientReportHtml(PatientReportData data)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<title>Patient Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #36b9cc; border-bottom: 2px solid #36b9cc; padding-bottom: 10px; }");
            html.AppendLine("h2 { color: #333; margin-top: 30px; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 15px; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            html.AppendLine(".summary { background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 15px 0; }");
            html.AppendLine(".summary-item { display: inline-block; margin: 10px 20px 10px 0; }");
            html.AppendLine(".summary-label { font-weight: bold; color: #333; }");
            html.AppendLine(".summary-value { color: #36b9cc; font-size: 18px; font-weight: bold; }");
            html.AppendLine(".active { color: #1cc88a; }");
            html.AppendLine(".inactive { color: #e74a3b; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Header
            html.AppendLine("<h1>Patient Report</h1>");
            html.AppendLine($"<p><strong>Report Period:</strong> {data.FromDate:MMMM dd, yyyy} to {data.ToDate:MMMM dd, yyyy}</p>");
            html.AppendLine($"<p><strong>Generated On:</strong> {DateTime.Now:MMMM dd, yyyy hh:mm tt}</p>");

            // Summary
            html.AppendLine("<div class='summary'>");
            html.AppendLine("<h2>Summary Statistics</h2>");
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>New Patients:</div>");
            html.AppendLine($"<div class='summary-value'>{data.NewPatients}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Total Patients:</div>");
            html.AppendLine($"<div class='summary-value'>{data.TotalPatients}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Active Patients:</div>");
            html.AppendLine($"<div class='summary-value active'>{data.ActivePatients}</div>");
            html.AppendLine("</div>");
            
            html.AppendLine($"<div class='summary-item'>");
            html.AppendLine($"<div class='summary-label'>Inactive Patients:</div>");
            html.AppendLine($"<div class='summary-value inactive'>{data.InactivePatients}</div>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");

            // Detailed Patients
            if (data.PatientDetails.Any())
            {
                html.AppendLine("<h2>Patient Details</h2>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>Patient ID</th><th>Name</th><th>Username</th><th>Registration Date</th><th>Status</th><th>Total Appointments</th></tr>");
                foreach (var patient in data.PatientDetails)
                {
                    var statusClass = patient.IsActive ? "active" : "inactive";
                    var status = patient.IsActive ? "Active" : "Inactive";
                    
                    html.AppendLine($"<tr>");
                    html.AppendLine($"<td>{patient.PatientID}</td>");
                    html.AppendLine($"<td>{patient.PatientName}</td>");
                    html.AppendLine($"<td>{patient.Username}</td>");
                    html.AppendLine($"<td>{patient.RegisteredDate:MMM dd, yyyy}</td>");
                    html.AppendLine($"<td class='{statusClass}'>{status}</td>");
                    html.AppendLine($"<td>{patient.TotalAppointments}</td>");
                    html.AppendLine("</tr>");
                }
                html.AppendLine("</table>");
            }

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}