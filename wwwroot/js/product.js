/*
Filename:  product.js
Purpose:   JavaScript used for Products Controller - Index View
Contains:  JavaScript to load the products data table and to delete products
Author:    Martin Dwyer
Created:   2022-08-17
Last Edit: 2022-08-23
By:        Martin Dwyer
*/

let dataTable;

// Loading products data table. See API documentation: https://datatables.net/reference/api/
function loadDataTable() {
    dataTable = $("#productData").DataTable({
        "ajax": {
            "url": "/Admin/Products/GetAll"
        },
        "columns": [
            {"data": "title", "width": "20%"},
            {"data": "isbn", "width": "10%"},
            {"data": "price", "width": "10%"},
            {"data": "author", "width": "15%"},
            {"data": "category.name", "width": "15%"},
            {
                "data": "id",
                "render": function (data) {
                    return `
                    
                        <a href="/Admin/Products/Upsert?id=${data}" class="btn btn-secondary">Edit</a> 
                        <a href="/Admin/Products/Details?id=${data}" class="btn btn-info">Details</a> 
                        <a onclick="return Delete('/Admin/Products/Delete?id=${data}')" class="btn btn-danger">Delete</a>
                    
                    `
                },
                "width": "25%"
            },
        ]
    });
}

// User interaction for deleting product.  See Sweet Alerts API: https://sweetalert.js.org/guides/
function Delete(url) {
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this product!",
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
                        swal("Poof! Your product was deleted!",
                            {
                                icon: "success",
                            });
                        dataTable.ajax.reload();
                    }
                });

            } else {
                swal("Your product is safe!");
            }
        });
}


$(document).ready(function () {
    loadDataTable();
});
