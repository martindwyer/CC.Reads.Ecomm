/*
Filename:  company.js
Purpose:   JavaScript used for Companies Controller - Index View
Contains:  JavaScript to load the companies data table and to delete companies
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

let dataTable;

// Loading companies data table. See API documentation: https://datatables.net/reference/api/
function loadDataTable() {
    dataTable = $("#productData").DataTable({
        "ajax": {
            "url": "/Admin/Companies/GetAll"
        },
        "columns": [
            {"data": "name", "width": "20%"},
            {"data": "streetAddress", "width": "10%"},
            {"data": "city", "width": "10%"},
            {"data": "state", "width": "10%"},
            {"data": "postalCode", "width": "10%"},
            {"data": "phoneNumber", "width": "10%"},
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <a href="/Admin/Companies/Upsert?id=${data}" class="btn btn-secondary">Edit</a> 
                        <a href="/Admin/Companies/Details?id=${data}" class="btn btn-info">Details</a> 
                        <a onclick="return Delete('/Admin/Companies/Delete?id=${data}')" class="btn btn-danger">Delete</a>
                    `
                },
                "width": "25%"
            },
        ]
    });
}

// User interaction for deleting company.  See Sweet Alerts API: https://sweetalert.js.org/guides/
function Delete(url) {
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this Company!",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    })
        .then((willDelete) => {
            if (willDelete) {
                $.ajax({
                    url: url,
                    type: 'DELETE',
                    success: function () {
                        swal("Poof! Company deleted!",
                            {
                                icon: "success",
                            });
                        dataTable.ajax.reload();
                    }
                });

            } else {
                swal("Your company is safe!");
            }
        });
}

// Load product data table when document is ready
$(document).ready(function () {
    loadDataTable();
});
