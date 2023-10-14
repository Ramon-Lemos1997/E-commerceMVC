$(document).ready(function () {
    $('#MyDataTables').DataTable({
        responsive: true,
        language: {

            "decimal": "",
            "emptyTable": "No data available in table",
            "info": "Mostrando _START_ registro de _END_ em um total de _TOTAL_ entrandas",
            "infoEmpty": "Showing 0 to 0 of 0 entries",
            "infoFiltered": "(filtered from _MAX_ total entries)",
            "infoPostFix": "",
            "thousands": ",",
            "lengthMenu": "Mostrar _MENU_ entradas",
            "loadingRecords": "Loading...",
            "processing": "",
            "search": "Procurar:",
            "zeroRecords": "No matching records found",
            "paginate": {
                "first": "Primeira",
                "last": "Última",
                "next": "Próximo",
                "previous": "Anterior"
            },
            "aria": {
                "sortAscending": ": activate to sort column ascending",
                "sortDescending": ": activate to sort column descending"
            }
        }
    });

    setTimeout((messageTemporayry) => {
        $(".alert").fadeOut("slow", () => {
            $(this).alert("close");
        })
    }, 5000)
})


// Verifica o tamanho do conteúdo e aplica classe CSS ao rodapé


//$(document).ready(function() {
//    $('#Emprestimos').DataTable({
//        paging: false,
//        scrollCollapse: true,
//        scrollY: '200px'
//    });
//});
