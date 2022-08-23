/*
Filename:  ErrorViewModel.cs
Purpose:   Provide a structure for Error View  
Contains:  Class properties limited to request id at the present time
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

namespace CC.Reads.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}