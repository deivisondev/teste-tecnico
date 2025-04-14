$(document).ready(function () {
    var beneficiarios = [];

    $('#CPF').on('input', function () {
        var valor = $(this).val();
        $(this).val(mascaraCPF(valor));
    });

    $('#CPFBeneficiario').on('input', function () {
        var valor = $(this).val();
        $(this).val(mascaraCPF(valor));
    });

    $('#formCadastro').submit(function (e) {
        e.preventDefault();
        $.ajax({
            url: urlPost,
            method: "POST",
            data: {
                "NOME": $(this).find("#Nome").val(),
                "CEP": $(this).find("#CEP").val(),
                "Email": $(this).find("#Email").val(),
                "Sobrenome": $(this).find("#Sobrenome").val(),
                "Nacionalidade": $(this).find("#Nacionalidade").val(),
                "Estado": $(this).find("#Estado").val(),
                "Cidade": $(this).find("#Cidade").val(),
                "Logradouro": $(this).find("#Logradouro").val(),
                "Telefone": $(this).find("#Telefone").val(),
                "CPF": $(this).find("#CPF").val(),
                "Beneficiarios": beneficiarios
            },
            error:
                function (r) {
                    if (r.status == 400)
                        ModalDialog("Ocorreu um erro", r.responseJSON);
                    else if (r.status == 500)
                        ModalDialog("Ocorreu um erro", "Ocorreu um erro interno no servidor.");
                },
            success:
                function (r) {
                    ModalDialog("Sucesso!", r);
                    $("#formCadastro")[0].reset();
                    beneficiarios = [];
                    atualizarTabela();
                }
        });
    });

    function atualizarTabela() {
        var tbody = $('#tabelaBeneficiarios tbody');
        tbody.empty();
        $.each(beneficiarios, function (index, beneficiario) {
            var tr = $('<tr>');
            tr.append('<td>' + beneficiario.cpf + '</td>');
            tr.append('<td>' + beneficiario.nome + '</td>');
            tr.append('<td><button class="btn btn-sm btn-primary alterar" data-index="' + index + '">Alterar</button> <button class="btn btn-sm btn-danger excluir" data-index="' + index + '">Excluir</button></td>');
            tbody.append(tr);
        });
    }

    function verificarCpfExistente(cpf) {
        if (cpf === $('#formCadastro').find("#CPF").val()) {
            return true;
        }

        return beneficiarios.some(function (beneficiario) {
            return beneficiario.cpf === cpf;
        });
    }

    $('#incluirBeneficiario').click(function () {
        var cpf = $('#CPFBeneficiario').val();
        var nome = $('#NomeBeneficiario').val().trim();

        if (verificarCpfExistente(cpf)) {
            alert('Este CPF já foi adicionado como cliente ou beneficiário.');
            return;
        }

        if (cpf.length === 14 && nome !== "") {
            beneficiarios.push({ cpf: cpf, nome: nome });
            atualizarTabela();
            $('#CPFBeneficiario').val('');
            $('#NomeBeneficiario').val('');
        } else {
            alert('CPF e Nome são obrigatórios.');
        }
    });

    $('#tabelaBeneficiarios').on('click', '.alterar', function () {
        var index = $(this).data('index');
        var beneficiario = beneficiarios[index];

        $('#CPFBeneficiario').val(beneficiario.cpf);
        $('#NomeBeneficiario').val(beneficiario.nome);
        beneficiarios.splice(index, 1);
        atualizarTabela();
    });

    $('#tabelaBeneficiarios').on('click', '.excluir', function () {
        var index = $(this).data('index');
        beneficiarios.splice(index, 1);
        atualizarTabela();
    });
});

function ModalDialog(titulo, texto) {
    var random = Math.random().toString().replace('.', '');
    var texto = '<div id="' + random + '" class="modal fade">                                                               ' +
        '        <div class="modal-dialog">                                                                                 ' +
        '            <div class="modal-content">                                                                            ' +
        '                <div class="modal-header">                                                                         ' +
        '                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>         ' +
        '                    <h4 class="modal-title">' + titulo + '</h4>                                                    ' +
        '                </div>                                                                                             ' +
        '                <div class="modal-body">                                                                           ' +
        '                    <p>' + texto + '</p>                                                                           ' +
        '                </div>                                                                                             ' +
        '                <div class="modal-footer">                                                                         ' +
        '                    <button type="button" class="btn btn-default" data-dismiss="modal">Fechar</button>             ' +
        '                                                                                                                   ' +
        '                </div>                                                                                             ' +
        '            </div><!-- /.modal-content -->                                                                         ' +
        '  </div><!-- /.modal-dialog -->                                                                                    ' +
        '</div> <!-- /.modal -->                                                                                        ';

    $('body').append(texto);
    $('#' + random).modal('show');
}

function mascaraCPF(cpf) {
    cpf = cpf.replace(/\D/g, "");

    if (cpf.length <= 3)
        return cpf;

    if (cpf.length <= 6)
        return cpf.replace(/(\d{3})(\d{0,})/, "$1.$2");

    if (cpf.length <= 9)
        return cpf.replace(/(\d{3})(\d{3})(\d{0,})/, "$1.$2.$3");

    if (cpf.length <= 10)
        return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{0,})/, "$1.$2.$3-$4");

    return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, "$1.$2.$3-$4");
}
