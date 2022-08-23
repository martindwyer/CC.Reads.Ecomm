/*
Filename:  order.js
Purpose:   JavaScript used for Order Controller
Contains:  JavaScript to load orders data table
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

let dataTable;

$(document).ready(function () {
    function loadDataTable(status) {
        dataTable = $("#orderData").DataTable({
            "ajax": {
                "url": "/Admin/Order/GetAll?status=" + status
            },
            "columns": [
                {"data": "id", "width": "5%"},
                {"data": "name", "width": "10%"},
                {"data": "phoneNumber", "width": "10%"},
                {"data": "applicationUser.email", "width": "15%"},
                {"data": "orderStatus", "width": "15%"},
                {"data": "paymentStatus", "width": "15%"},
                {"data": "orderTotal", "width": "15%"},
                {
                    "data": "id",
                    "render": function (data) {
                        return ` 
                        <a href="/Admin/Order/Details?id=${data}" class="btn btn-info">Details</a> 
                    `
                    },
                    "width": "25%"
                },
            ]
        });
    }

    let status = document.getElementById("status").textContent;

    loadDataTable(status);
});
    



