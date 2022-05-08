﻿using DevInSales.Context;
using DevInSales.DTOs;
using DevInSales.Models;
using DevInSales.Context;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DevInSales.Controllers
{
    [Route("api/product")]
    [ApiController]

    public class ProductController : ControllerBase
    {
        private readonly SqlContext _sqlContext;

        public ProductController(SqlContext sqlContext)
        {
            _sqlContext = sqlContext;
        }

        /// <summary>
        /// Consulta a lista de Produtos
        /// </summary>
        /// <param name="name">Filtra o produto por nome</param>
        /// <param name="price_min">Delimita um preço mínimo na consulta do produto</param>
        /// <param name="price_max">Delimita um preço máximo na consulta do produto</param>
        /// <returns>Retorna lista de produtos consultados.</returns>
        /// <response code="200">Retorno da lista de produto(s) consultado(s).</response>
        /// <response code="204">Sem nenhum retorno.</response>
        /// <response code="400">Erro ao fazer a Request.</response>
        [HttpGet(Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ProductGetDTO>>> GetProduct(string? name, decimal? price_min, decimal? price_max)
        {
            if (price_max < price_min)
                return BadRequest($"O Preço Máximo ({price_max}) não pode ser menor que o Preço Mínimo ({price_min}).");

            var productQuery = _sqlContext.Product as IQueryable<Product>;
            if (!string.IsNullOrWhiteSpace(name))
                productQuery = productQuery.Where(p => p.Name.Contains(name));

            if (price_min.HasValue)
                productQuery = productQuery.Where(p => p.Suggested_Price >= price_min.Value);

            if (price_max.HasValue)
                productQuery = productQuery.Where(p => p.Suggested_Price <= price_max.Value);

            List<Product> productResult = productQuery
              .Include(x => x.Category)
              .ToList();

            var retorno = new List<ProductGetDTO>();
            foreach (var product_unit in productResult)
            {
                retorno.Add((ProductGetDTO)product_unit);
            }

            return (retorno?.Any() ?? false) ? Ok(retorno) : NoContent();
        }

        /// <summary>
        /// Cadastra um novo Produto
        /// </summary>
        /// <param name="requisicao">Representa as informações do novo Produto.</param>
        /// <returns>Retorna o novo Produto cadastrado.</returns>
        /// <response code="201">Insercao realizada com sucesso.</response>
        /// <response code="400">Produto com mesmo nome já cadastrado, ou preço sugerido menor ou igual à 0.</response>
        /// <response code="404">Produto não encontrado.</response>
        /// <response code="500">Ocorreu uma exceção durante o cadastro.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] ProductPostAndPutDTO requisicao)
        {
            bool isProductExistente = _sqlContext.Product.Any(product => product.Name == requisicao.Name);
            
            if (isProductExistente)
                return BadRequest("O nome do produto já existe.");

            if (requisicao.Suggested_Price <= 0)
                return BadRequest("O preço sugerido não pode ser menor ou igual a 0.");

            var newProduct = ProductPostAndPutDTO.ConverterParaEntidade(requisicao);
            _sqlContext.Product.Add(newProduct);
            await _sqlContext.SaveChangesAsync();

            return CreatedAtAction("Create", new { id = newProduct.Id }, newProduct);
        }

        /// <summary>
        /// Atualiza um Produto
        /// </summary>
        /// <param name="id">Representa o Id do Produto.</param>
        /// <param name="requisicao">Representa as informações que serão atualizadas do Produto.</param>
        /// <response code="204">Atualização realizada com sucesso.</response>
        /// <response code="400">Já existe um outro produto com o nome a ser modificado, ou Preço sugerido menor ou igual à 0, ou o Nome e Preço não foram inserios.</response>
        /// <response code="404">Id do Produto não foi encontrado.</response>
        /// <response code="500">Ocorreu uma exceção durante a atualização.</response>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Product>> PutProduct(int id, ProductPostAndPutDTO requisicao)
        {
            bool productIdExists = _sqlContext.Product.Any(product => product.Id == id);
            bool productNameExists = _sqlContext.Product.Any(product => product.Name == requisicao.Name);

            if (!productIdExists)
                return NotFound("O produto não existe.");

            if (productNameExists)
                return BadRequest("Já existeu um produto com este nome.");

            if (requisicao.Suggested_Price <= 0)
                return BadRequest("O preço sugerido não pode ser menor ou igual a 0.");

            if (requisicao.Name == null || requisicao.Suggested_Price == null)
                return BadRequest("Nome ou Preço Sugerido são Nulos.");

            var newProduct = ProductPostAndPutDTO.ConverterParaEntidade(requisicao, id);
            _sqlContext.Entry(newProduct).State = EntityState.Modified;

            try
            {
                await _sqlContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductIdExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return NoContent();
        }

        private bool ProductIdExists(int id)
        {
            return _sqlContext.Product.Any(e => e.Id == id);
        }

        /// <summary>
        ///atualiza o nome e preço do produto
        /// </summary>
        /// <param name="productModel"></param>
        /// <param name="id"></param>
        /// <returns>atualiza o nome e preço do produto</returns>
        /// <response code="200">Produto Atualizado</response>
        /// <response code="204">Sem nenhum retorno.</response>
        /// <response code="400">Erro ao fazer a Request.</response>
        /// <response code="404">produto inexistente</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchProduct(JsonPatchDocument productModel, int id)
        {
            var product = await _sqlContext.Product.FindAsync(id);

            if (product == null)
            {
                return StatusCode(404);
            }

            if (product != null)
            {
                productModel.ApplyTo(product);
                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    return StatusCode(404);
                }
                if (product.Suggested_Price <= 0)
                {
                    return StatusCode(404);
                }
                await _sqlContext.SaveChangesAsync();
                return StatusCode(204);
            }
            return StatusCode(200);
        }

        /// <summary>
        /// Deleta um produto conforme o Id Informado.
        /// </summary>
        /// <param name="product_id">O Id do produto para ser deletado.</param>
        /// <returns>Deleta o produto conforme o Id informado.</returns>
        /// <response code="200">Produto deletado.</response>
        /// <response code="400">Produto com Ordem de Produto Vinculada.</response>
        /// <response code="404">Produto não encontrado.</response>
        [HttpDelete("{product_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct([FromRoute] int product_id)
        {
            var productIdEncontrado = await _sqlContext.Product.FindAsync(product_id);
            if (productIdEncontrado == null)
                return NotFound($"O Id de Produto de número {product_id} não foi encontrado.");

            bool haveOrderProduct = await _sqlContext.Order_Product.AnyAsync(op => op.Products.Id == product_id);
            if (haveOrderProduct)
                return BadRequest($"O Id de Produto de número {product_id} possui uma Ordem de Produto vinculada, por este motivo não pode ser deletado.");

            _sqlContext.Product.Remove(productIdEncontrado);
            _sqlContext.SaveChanges();
            return Ok(product_id);
        }
    }
}