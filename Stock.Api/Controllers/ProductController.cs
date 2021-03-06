using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Stock.Api.DTOs;
using Stock.Api.Extensions;
using Stock.AppService.Services;
using Stock.Model.Entities;

namespace Stock.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private ProductService service;
        private readonly IMapper mapper;
        private ProductTypeService productTypeService;
        private ProviderService providerService;



        public ProductController(ProductService service, IMapper mapper, ProductTypeService productTypeService, ProviderService providerService)
        {
            this.service = service;
            this.mapper = mapper;
            this.productTypeService = productTypeService;
            this.providerService = providerService;
        }
        /// <summary>
        /// Permite crear una nueva instancia
        /// </summary>
        /// <param name="value">Una instancia</param>
        [HttpPost]
        public ActionResult Post([FromBody] ProductDTO value)
        {
            var product = this.mapper.Map<Product>(value);
            var producttype = this.productTypeService.Get(value.ProductTypeId);
            if (producttype != null)
            {
                product.ProductType = producttype;
                this.service.Create(product);
                value.Id = product.Id;
                value.ProductTypeDesc = producttype.Description;
                return Ok(new { Success = true, Message = "", data = value });
            }
            else
            {
                return Ok(new { Success = false, Message = "Tipo requerido." });
            }
        }




        /// <summary>
        /// Permite recuperar todas las instancias
        /// </summary>
        /// <returns>Una colección de instancias</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ProductDTO>> Get()
        {
            try
            {
                var result = this.service.GetAll();
                return this.mapper.Map<IEnumerable<ProductDTO>>(result).ToList();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Permite obtener una instancia de Productos
        /// </summary>
        /// <param name="id">Identificador de la instancia a obtener</param>
        [HttpGet("{id}")]
        public ActionResult<ProductDTO> Get(string id)
        {
            try
            {
                var result = this.service.Get(id);
                return this.mapper.Map<ProductDTO>(result);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }
        /// <summary>
        /// Permite actualizar una instancia de Productos
        /// </summary>
        /// <param name="id">Identificador de la instancia a actulizar</param>
        /// <param name="value">Valores a actulizar</param>
        [HttpPut("{id}")]
        public ActionResult Put(string id, [FromBody] ProductDTO value)
        {
            var product = this.service.Get(id);
            this.mapper.Map<ProductDTO, Product>(value, product);
            var producttype = this.productTypeService.Get(value.ProductTypeId);
            if (producttype != null)
            {
                product.ProductType = producttype;
                this.service.Update(product);
                value.Id = product.Id;
                value.ProductTypeDesc = producttype.Description;
                return Ok(new { Success = true, Message = "", data = value });
            }
            else
            {
                return Ok(new { Success = false, Message = "Se requiere una categoría que exista." });
            }

        }

        /// <summary>
        /// Permite borrar una instancia
        /// </summary>
        /// <param name="id">Identificador de la instancia a borrar</param>
        [HttpDelete("{id}")]
        public ActionResult Delete(string id)
        {
            try
            {
                var product = this.service.Get(id);
                this.service.Delete(product);
                return Ok(new { Success = true, Message = "", data = id });
            }
            catch
            {
                return Ok(new { Success = false, Message = "", data = id });
            }
        }




        /// <summary>
        /// Permite buscar una instancia
        /// </summary>
        /// <param name="model">Identificador de la instancia a bucar por nombre o precio de salida</param>

        [HttpPost("search")]
        public ActionResult Search([FromBody] ProductSearchDTO model)
        {
            Expression<Func<Product, bool>> filter = x => !string.IsNullOrWhiteSpace(x.Id);

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                filter = filter.AndOrCustom(
                    x => x.Name.ToUpper().Contains(model.Name.ToUpper()),
                    model.Condition.Equals(ActionDto.AND));
            }

            if (!string.IsNullOrWhiteSpace(model.ProductTypeDesc))
            {
                filter = filter.AndOrCustom(
                    x => x.ProductType != null,
                    model.Condition.Equals(ActionDto.AND));

                filter = filter.AndOrCustom(
                    x => x.ProductType.Description.ToUpper().Contains(model.ProductTypeDesc.ToUpper()),
                    model.Condition.Equals(ActionDto.AND));
            }
            var products = this.service.Search(filter);
            var mappedProducts = this.mapper.Map<IEnumerable<ProductDTO>>(products);
            return Ok(mappedProducts);
        }
    }
}