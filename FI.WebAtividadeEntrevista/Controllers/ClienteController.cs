using FI.AtividadeEntrevista.BLL;
using WebAtividadeEntrevista.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FI.AtividadeEntrevista.DML;

namespace WebAtividadeEntrevista.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Incluir(ClienteModel model)
        {
            try
            {
                BoCliente boCliente = new BoCliente();
                BoBeneficiario boBeneficiario = new BoBeneficiario();

                if (!this.ModelState.IsValid)
                {
                    List<string> erros = (from item in ModelState.Values
                                          from error in item.Errors
                                          select error.ErrorMessage).ToList();

                    throw new Exception(string.Join(Environment.NewLine, erros));
                }

                if (boCliente.VerificarExistencia(model.CPF))
                    throw new Exception("Já existe um cadastro com este CPF");

                foreach (var beneficiario in model.Beneficiarios)
                {
                    if (model.CPF == beneficiario.CPF)
                        throw new Exception("O CPF do cliente não pode ser igual ao a um beneficiario atrelado");
                }

                model.Id = boCliente.Incluir(new Cliente()
                {
                    CEP = model.CEP,
                    Cidade = model.Cidade,
                    Email = model.Email,
                    Estado = model.Estado,
                    Logradouro = model.Logradouro,
                    Nacionalidade = model.Nacionalidade,
                    Nome = model.Nome,
                    Sobrenome = model.Sobrenome,
                    Telefone = model.Telefone,
                    CPF = model.CPF,
                });

                foreach (var beneficiario in model.Beneficiarios)
                {
                    boBeneficiario.Incluir(new Beneficiario()
                    {
                        CPF = beneficiario.CPF,
                        Nome = beneficiario.Nome,
                        IdCliente = model.Id
                    });
                }

                return Json("Cadastro efetuado com sucesso");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 400;
                return Json(ex.Message);
            }
        }

        [HttpPost]
        public JsonResult Alterar(ClienteModel model)
        {
            try
            {
                BoCliente boCliente = new BoCliente();
                BoBeneficiario boBeneficiario = new BoBeneficiario();

                if (!this.ModelState.IsValid)
                {
                    List<string> erros = (from item in ModelState.Values
                                          from error in item.Errors
                                          select error.ErrorMessage).ToList();


                    throw new Exception(string.Join(Environment.NewLine, erros));
                }

                Cliente cliente = boCliente.Consultar(model.Id);

                if (cliente.CPF != model.CPF && boCliente.VerificarExistencia(model.CPF))
                    throw new Exception("Já existe um cadastro com este CPF");

                foreach (var beneficiario in model.Beneficiarios)
                {
                    if (model.CPF == beneficiario.CPF)
                        throw new Exception("O CPF do cliente não pode ser igual ao a um beneficiario atrelado");

                    bool existe = boBeneficiario.VerificarExistencia(beneficiario.CPF, model.Id);

                    if (existe)
                    {
                        string mensagem = "Já existe um beneficiário com o CPF " + beneficiario.CPF;

                        if (beneficiario.Id != 0)
                        {
                            Beneficiario ben = boBeneficiario.Consultar(beneficiario.Id);

                            if (beneficiario.CPF != ben.CPF)
                                throw new Exception(mensagem);
                        }
                        else
                            throw new Exception(mensagem);
                    }
                }

                boCliente.Alterar(new Cliente()
                {
                    Id = model.Id,
                    CEP = model.CEP,
                    Cidade = model.Cidade,
                    Email = model.Email,
                    Estado = model.Estado,
                    Logradouro = model.Logradouro,
                    Nacionalidade = model.Nacionalidade,
                    Nome = model.Nome,
                    Sobrenome = model.Sobrenome,
                    Telefone = model.Telefone,
                    CPF = model.CPF
                });

                foreach (var beneficiario in model.Beneficiarios)
                {
                    if (beneficiario.Id != 0)
                    {
                        boBeneficiario.Alterar(new Beneficiario()
                        {
                            Id = beneficiario.Id,
                            Nome = beneficiario.Nome,
                            CPF = beneficiario.CPF,
                            IdCliente = beneficiario.IdCliente
                        });
                    }
                    else
                    {
                        boBeneficiario.Incluir(new Beneficiario()
                        {
                            Nome = beneficiario.Nome,
                            CPF = beneficiario.CPF,
                            IdCliente = model.Id
                        });
                    }
                }

                foreach (var id in model.BeneficiariosDeletados)
                {
                    boBeneficiario.Excluir(id);
                }

                return Json("Cadastro alterado com sucesso");
            }
            catch (Exception ex)
            {
                Response.StatusCode = 400;
                return Json(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            BoCliente bo = new BoCliente();
            BoBeneficiario boBeneficiario = new BoBeneficiario();
            Models.ClienteModel model = null;

            Cliente cliente = bo.Consultar(id);
            List<Beneficiario> beneficiarios = boBeneficiario.ListarPorIdCliente(id);
            List<long> beneficiariosDeletados = new List<long>();

            if (beneficiarios != null && cliente != null)
            {
                model = new ClienteModel()
                {
                    Id = cliente.Id,
                    CEP = cliente.CEP,
                    Cidade = cliente.Cidade,
                    Email = cliente.Email,
                    Estado = cliente.Estado,
                    Logradouro = cliente.Logradouro,
                    Nacionalidade = cliente.Nacionalidade,
                    Nome = cliente.Nome,
                    Sobrenome = cliente.Sobrenome,
                    Telefone = cliente.Telefone,
                    CPF = cliente.CPF,
                    Beneficiarios = beneficiarios.Select((beneficiario) => new BeneficiarioModel()
                    {
                        Id = beneficiario.Id,
                        Nome = beneficiario.Nome,
                        CPF = beneficiario.CPF,
                        IdCliente = beneficiario.IdCliente
                    }).ToList(),
                    BeneficiariosDeletados = beneficiariosDeletados
                };
            }

            return View(model);
        }

        [HttpPost]
        public JsonResult ClienteList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                int qtd = 0;
                string campo = string.Empty;
                string crescente = string.Empty;
                string[] array = jtSorting.Split(' ');

                if (array.Length > 0)
                    campo = array[0];

                if (array.Length > 1)
                    crescente = array[1];

                List<Cliente> clientes = new BoCliente().Pesquisa(jtStartIndex, jtPageSize, campo, crescente.Equals("ASC", StringComparison.InvariantCultureIgnoreCase), out qtd);

                //Return result to jTable
                return Json(new { Result = "OK", Records = clientes, TotalRecordCount = qtd });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
    }
}