// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//$(document).ready(function () {
//    $('#TableData').DataTable({
//        dom: 'Bfrtip',
//        buttons: [
//            { extend: 'pdf', className: 'btn btn-primary' },
//            { extend: 'print', className: 'btn btn-primary' },
//            { extend: 'copy', className: 'btn btn-primary' },
//            { extend: 'csv', className: 'btn btn-primary' },
//            { extend: 'excel', className: 'btn btn-primary' }
//        ]
//    });
//});

//Bootstrap

$('#myList a').on('click', function (e) {
e.preventDefault()
$(this).tab('show')
})



//Stop Inspecting Element

//$(document).bind("contextmenu", function (e) {
//    e.preventDefault();
//});

//document.onkeydown = function (e) {
//    if (e.keyCode == 123) {
//        return false;
//    }
//    if (e.ctrlKey && e.shiftKey && e.keyCode == 'I'.charCodeAt(0)) {
//        return false;
//    }
//    if (e.ctrlKey && e.shiftKey && e.keyCode == 'J'.charCodeAt(0)) {
//        return false;
//    }
//    if (e.ctrlKey && e.keyCode == 'U'.charCodeAt(0)) {
//        return false;
//    }

//    if (e.ctrlKey && e.shiftKey && e.keyCode == 'C'.charCodeAt(0)) {
//        return false;
//    }
//}