using E_Commerce.Models;
using E_Commerce.Repositories;
using E_Commerce.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Product = E_Commerce.Models.Product;
using ProductDTO = E_Commerce.Models.ProductDTO;
namespace E_Commerce.Services
{
    
    public class VendorService : IVendorService
    {
        private readonly IProductRepository _productRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IElasticsearchService<ProductItem> _elasticsearchService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VendorService> _logger;
        private readonly IMemberRepository<ProductDTO> _productCache;
        private readonly IMemberRepository<CategoryDTO> _categoryCache;


        public VendorService(
            IProductRepository productRepository,
            IVendorRepository vendorRepository,
            ICategoryRepository categoryRepository,
            IElasticsearchService<ProductItem> elasticsearchService,
            IUnitOfWork unitOfWork,
            IMemberRepository<ProductDTO> productCache,
            IMemberRepository<CategoryDTO> categoryCache,
            ILogger<VendorService> logger)
        {
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _categoryRepository = categoryRepository;
            _elasticsearchService = elasticsearchService;
            _unitOfWork = unitOfWork;
            _productCache = productCache;
            _categoryCache = categoryCache;
            _logger = logger;
        }
        public async Task<OperationResult<List<ProductDTO>>> GetVendorProductsAsync(string vendorId, bool forceRefresh = false)
        {
           try
            {
                var cacheKey = $"vendor_products_{vendorId}";
                var cachedProducts = await _productCache.GetMemberListAsync(cacheKey);
                if (!forceRefresh && cachedProducts.Count>0)
                {
                    return OperationResult<List<ProductDTO>>.SuccessResult(cachedProducts);
                }
                _logger.LogDebug("Getting data from database...");
                if (!Guid.TryParse(vendorId, out Guid parsedVendorId))
                    return OperationResult<List<ProductDTO>>.FailureResult(400, "Invalid vendor ID");

                var productsResult = await _productRepository.GetProductsAsync(parsedVendorId);
                if (productsResult==null || !productsResult.Success || productsResult.Data == null)
                {

                    return OperationResult<List<ProductDTO>>.FailureResult(
                        productsResult?.StatusCode ?? 500,
                        productsResult?.Errors?.Any() == true ? string.Join(", ", productsResult.Errors) : "Vendor not found");
                }
                var data = productsResult.Data;
                var products = data.Select(p => new ProductDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    CategoryName = p.CategoryName
                }).ToList();



                await _productCache.AddMemberListAsync(cacheKey, products);
                return OperationResult<List<ProductDTO>>.SuccessResult(products);
            }
            catch (Exception ex)
            {
               
                return OperationResult<List<ProductDTO>>.FailureResult(500, ex.Message);
            }
        }
        public async Task<OperationResult<ProductDTO>> AddProductAsync(string vendorId, ProductModel product)
        {
            if (product == null)
                return OperationResult<ProductDTO>.FailureResult(400, "Product is null.");

            try
            {
                await _unitOfWork.StartTransactionAsync();
                var session=_unitOfWork.Session;

                var categoryResult = await _categoryRepository.GetCategoryByNameAsync(product.CategoryName!);

                if (categoryResult == null || !categoryResult.Success || categoryResult.Data == null)
                {
                    await _unitOfWork.AbortAsync();
                    var error = categoryResult?.Errors != null
                        ? string.Join(", ", categoryResult.Errors)
                        : "Something went wrong";
                    return OperationResult<ProductDTO>.FailureResult(categoryResult?.StatusCode ?? 500, error);
                }
            
                if (!Guid.TryParse(vendorId, out Guid parsedVendorId))
                {
                    await _unitOfWork.AbortAsync();
                    return OperationResult<ProductDTO>.FailureResult(400, "Invalid parameters");
                }

                var vendorResult = await _vendorRepository.GetVendorByIdAsync(parsedVendorId);
                if (vendorResult == null || !vendorResult.Success || vendorResult.Data == null)
                {
                    await _unitOfWork.AbortAsync();
                    var error = vendorResult?.Errors != null
                        ? string.Join(", ", vendorResult.Errors)
                        : "Something went wrong";
                    return OperationResult<ProductDTO>.FailureResult(vendorResult?.StatusCode ?? 500, error);
                }


                var newProduct = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    CategoryId = categoryResult.Data.Id,
                    VendorId = vendorResult.Data.UserId,
                    CategoryName = product.CategoryName,
                };

                var addProductResult = await _productRepository.AddProductAsync(newProduct, session);
                if (addProductResult == null || !addProductResult.Success || addProductResult.Data == null)
                {
                    await _unitOfWork.AbortAsync();
                    var error = addProductResult?.Errors != null
                        ? string.Join(", ", addProductResult.Errors)
                        : "Something went wrong";
                    return OperationResult<ProductDTO>.FailureResult(addProductResult?.StatusCode ?? 500, error);
                }
            
                if (string.IsNullOrWhiteSpace(newProduct.Id.ToString()) || string.IsNullOrWhiteSpace(newProduct.CategoryId.ToString()))
                {
                    await _unitOfWork.AbortAsync();
                    return OperationResult<ProductDTO>.FailureResult(400, "ProductId and CategoryId must not be null or empty.");
                }
                if (!Guid.TryParse(newProduct.Id.ToString(), out Guid parsedProductId) || !Guid.TryParse(newProduct.CategoryId.ToString(), out Guid parsedCategoryId))
                {
                    await _unitOfWork.AbortAsync();
                    return OperationResult<ProductDTO>.FailureResult(400, "Invalid GUID format for productId or categoryId.");
                }
             

                

  
                if (string.IsNullOrWhiteSpace(newProduct.Id.ToString()) || string.IsNullOrWhiteSpace(vendorId))
                {
                    await _unitOfWork.AbortAsync();
                    return OperationResult<ProductDTO>.FailureResult(400, "Invalid parameters");
                }

                if (!Guid.TryParse(newProduct.Id.ToString(), out Guid productId))
                {
                    await _unitOfWork.AbortAsync();
                    return OperationResult<ProductDTO>.FailureResult(400, "Invalid parmaters");
                }
                await _unitOfWork.CommitAsync();

                var productItem = new ProductItem
                {
                    Id = newProduct.Id,
                    VendorId = newProduct.VendorId,
                    Name = newProduct.Name,
                    Price = newProduct.Price,
                    CategoryName = newProduct.CategoryName,
                    Quantity = newProduct.Quantity,
                    VendorName = vendorResult.Data.UserName

                };
                var elasticSearchResult = await _elasticsearchService.CreateAsync(productItem);
                if (elasticSearchResult == null || !elasticSearchResult.Success || elasticSearchResult.Data == null)
                {
                    var error = elasticSearchResult?.Errors != null
                        ? string.Join(", ", elasticSearchResult.Errors)
                        : "Something went wrong ";

                    return OperationResult<ProductDTO>.FailureResult(elasticSearchResult?.StatusCode ?? 500, error);
                }

      

                var cacheKey = $"vendor_products_{vendorId}";

                var products = await _productCache.GetMemberListAsync(cacheKey);
                var result = new ProductDTO
                {
                    Id = newProduct.Id,
                    Name = newProduct.Name,
                    Quantity = newProduct.Quantity,
                    Price = newProduct.Price,
                    CategoryName = newProduct.CategoryName,
                };
                products.Add(result);
                
                await _productCache.UpdateMemberListAsync(cacheKey, products);
             

                return OperationResult<ProductDTO>.SuccessResult(new ProductDTO
                {
                    Id = newProduct.Id,
                    Name = newProduct.Name,
                    Quantity = newProduct.Quantity,
                    Price = newProduct.Price,
                    CategoryName = product.CategoryName,
                });
            }
            catch (Exception ex)
            {
                await _unitOfWork.AbortAsync();
                return OperationResult<ProductDTO>.FailureResult(500, ex.Message);
            }
        }
        public async Task<OperationResult<List<CategoryDTO>>> GetAllCategoriesAsync()
        {
            try
            {
                var cacheKey = "categories";
                var cacheCategories = await _categoryCache.GetMemberListAsync(cacheKey);
                if (cacheCategories.Count >0)
                {
                    return OperationResult<List<CategoryDTO>>.SuccessResult(cacheCategories);
                 
                }
                var getCategoriesResult = await _categoryRepository.GetAllCategoriesAsync();
                if (getCategoriesResult == null || !getCategoriesResult.Success || getCategoriesResult.Data == null)
                {
                    var error = getCategoriesResult?.Errors != null
                        ? string.Join(", ", getCategoriesResult.Errors)
                        : "Something went wrong ";

                    return OperationResult<List<CategoryDTO>>.FailureResult(getCategoriesResult?.StatusCode ?? 500, error);
                }
                var categoriesResult = getCategoriesResult.Data;

                var categories = categoriesResult.Select(c => new CategoryDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                }).ToList();

                var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(59))
                .SetAbsoluteExpiration(TimeSpan.FromHours(23));

               await _categoryCache.AddMemberListAsync(cacheKey, categories);
                return OperationResult<List<CategoryDTO>>.SuccessResult(categories);
            }
            catch (Exception ex)
            {
       
                return OperationResult<List<CategoryDTO>>.FailureResult(500, ex.Message);
            }

        }

        public async Task<OperationResult<ProductDTO>> GetProductAsync(string vendorId, string productId)
        {
            try
            {
                if (!Guid.TryParse(productId, out Guid parsedProductId) || !Guid.TryParse(vendorId, out Guid parsedVendorId))
                    return OperationResult<ProductDTO>.FailureResult(400, "Invalid parameters");


                var cacheKey = $"vendor_products_{vendorId}";
                var cachedProducts=await _productCache.GetMemberListAsync(cacheKey);
                if(cachedProducts.Count>0)
                {
                    var productCached = cachedProducts.FirstOrDefault(prod => prod.Id == parsedProductId);
                    return OperationResult<ProductDTO>.SuccessResult(productCached!);
                }
                var getProductResult=await _productRepository.GetProductAsync(parsedVendorId,parsedProductId);
                if (!getProductResult.Success)
                    return OperationResult<ProductDTO>.FailureResult(getProductResult.StatusCode, String.Join("," ,getProductResult.Errors));
                var product = new ProductDTO
                {
                    Id = getProductResult.Data!.Id,
                    Name = getProductResult.Data.Name,
                    Quantity = getProductResult.Data.Quantity,
                    CategoryName = getProductResult.Data.CategoryName,
                    Price = getProductResult.Data.Price,

                };
                return OperationResult<ProductDTO>.SuccessResult(product);

            }
            catch (Exception ex)
            {

                return OperationResult<ProductDTO>.FailureResult(500, ex.Message);
            }
        }
        public async Task<OperationResult<ProductDTO>> UpdateProductAsync(string vendorId,string productId,
            ProductModel product)
        {
            try
            {
                await _unitOfWork.StartTransactionAsync();
                var session=_unitOfWork.Session;
                if (!Guid.TryParse(productId, out Guid parsedProductId) || !Guid.TryParse(vendorId, out Guid parsedVendorId))
                {
                    await _unitOfWork.AbortAsync();
                    return OperationResult<ProductDTO>.FailureResult(400, "Invalid parameters");
                }

            
                
                var categories = await _categoryCache.GetMemberListAsync("categories");
                if(categories.Count==0)
                {
                    var getCategoriesResult = await _categoryRepository.GetAllCategoriesAsync();
                    if(getCategoriesResult==null)
                        return OperationResult<ProductDTO>.FailureResult(500, "Something went wrong");
                    if (!getCategoriesResult.Success)
                        return OperationResult<ProductDTO>.FailureResult(getCategoriesResult.StatusCode,
                           String.Join(", ",getCategoriesResult.Errors));
                    categories=getCategoriesResult.Data!.Select(c=> new CategoryDTO
                    {
                        Id = c.Id,
                        Name = c.Name,
                    
                    }).ToList();
                }
          
                var newCategory = categories.Find(c => c.Name == product.CategoryName);
         
            
                var productUpdated = new Product
                {
                    Id = parsedProductId,
                    Name = product.Name,
                    Quantity = product.Quantity,
                    CategoryName = newCategory!.Name,
                    Price = product.Price,
                    VendorId = parsedVendorId,
                    CategoryId = newCategory!.Id
                };
             
                var updateProductResult = await _productRepository.EditProductAsync(parsedVendorId, parsedProductId, productUpdated, session);
                await _unitOfWork.CommitAsync();

                var data = updateProductResult.Data;
            
                var getProductELasticSearch = await _elasticsearchService.GetAsync(productId);
                if (getProductELasticSearch == null || !getProductELasticSearch.Success || getProductELasticSearch.Data == null)
                {
                    var error = getProductELasticSearch?.Errors != null
                        ? string.Join(", ", getProductELasticSearch.Errors)
                        : "Something went wrong ";

                    return OperationResult<ProductDTO>.FailureResult(getProductELasticSearch?.StatusCode ?? 500, error);
                }

;                var productItem = new ProductItem
                {
                    Id = parsedProductId,
                    VendorId =parsedVendorId,
                    Name = data.Name,
                    Price = data.Price,
                    CategoryName = data.CategoryName,
                    Quantity = data.Quantity,
                    VendorName = getProductELasticSearch.Data.VendorName

                };
                var updateProductELasticSearch = await _elasticsearchService.UpdateAsync(productItem);
                if (updateProductELasticSearch == null || !updateProductELasticSearch.Success || updateProductELasticSearch.Data == null)
                {
                    var error = updateProductELasticSearch?.Errors != null
                        ? string.Join(", ", updateProductELasticSearch.Errors)
                        : "Something went wrong ";

                    return OperationResult<ProductDTO>.FailureResult(updateProductELasticSearch?.StatusCode ?? 500, error);
                }

                var result = new ProductDTO
                {
                    Id = data!.Id,
                    Name = data.Name,
                    Price = data.Price,
                    Quantity = data.Quantity,
                    CategoryName = data.CategoryName,

                };
               

                var cacheKey = $"vendor_products_{vendorId}";
                var products = await _productCache.GetMemberListAsync(cacheKey);
                var index = products.FindIndex(product => product.Id == parsedProductId);
                if (index != -1)
                {
                    products[index] = result;
                    await _productCache.UpdateMemberListAsync(cacheKey, products);
                }

                await _categoryCache.UpdateMemberListAsync("categories", categories);
                return OperationResult<ProductDTO>.SuccessResult(result);

            }
            catch (Exception ex)
            {
                await _unitOfWork.AbortAsync();

                return OperationResult<ProductDTO>.FailureResult(500, ex.Message);
            }
          
        }
        public async Task<OperationResult<ProductDTO>> DeleteProductAsync(string vendorId, string productId)
        {
            try
            {
                await _unitOfWork.StartTransactionAsync();
                var session=_unitOfWork.Session;
                if (!Guid.TryParse(productId, out Guid parsedProductId) || 
                    !Guid.TryParse(vendorId, out Guid parsedVendorId))
                    return OperationResult<ProductDTO>.FailureResult(400, "Invalid parameters");
                 var cacheKey = $"vendor_products_{vendorId}";
                var products = await _productCache.GetMemberListAsync(cacheKey);
                var product= products.Find(
                     p => p.Id == parsedProductId);


                if (product == null)
                {
                    var getProductResult=await _productRepository.GetProductAsync(parsedProductId);
                    if (getProductResult == null || !getProductResult.Success || getProductResult.Data == null)
                    {
                        await _unitOfWork.AbortAsync();
                        var error = getProductResult?.Errors != null
                            ? string.Join(", ", getProductResult.Errors)
                            : "Something went wrong ";

                        return OperationResult<ProductDTO>.FailureResult(getProductResult?.StatusCode ?? 500, error);
                    }
                    var data= getProductResult.Data;
                    if (data.VendorId != parsedVendorId)
                    {
                        await _unitOfWork.AbortAsync();
                        return OperationResult<ProductDTO>.FailureResult(403, "You are not allowed to perform this operation.");
                    }
                    product = new ProductDTO()
                    {
                        Id = data.Id,
                        Name = data.Name,
                        Price = data.Price,
                        Quantity = data.Quantity,
                        CategoryName = data.CategoryName,
                    };
                }
                var categories = await _categoryCache.GetMemberListAsync("categories");
                var category=categories.Find(c => c.Name == product.CategoryName);
                if (category==null)
                {
                    var getCategoryResult= await _categoryRepository.GetCategoryByNameAsync(product.CategoryName!);
                    if (getCategoryResult == null)
                    {

                        return OperationResult<ProductDTO>.FailureResult(500, "Something went wrong");
                    }
                    if (!getCategoryResult.Success)
                    {
                        var error = String.Join(", ", getCategoryResult.Errors);
                        return OperationResult<ProductDTO>.FailureResult(getCategoryResult.StatusCode, error);
                    }
                    if (getCategoryResult.Data == null)
                    {

                        return OperationResult<ProductDTO>.FailureResult(500, "Something went wrong");
                    }
                    category = new CategoryDTO()
                    {
                        Id = getCategoryResult.Data.Id,
                        Name = getCategoryResult.Data.Name,
                    };
                }

                var deleteProductResult=await _productRepository.DeleteProductAsync(parsedVendorId,parsedProductId, session);
                if (deleteProductResult == null || !deleteProductResult.Success)
                {
                    await _unitOfWork.AbortAsync();
                    var error = deleteProductResult?.Errors != null
                        ? string.Join(", ", deleteProductResult.Errors)
                        : "Something went wrong ";

                    return OperationResult<ProductDTO>.FailureResult(deleteProductResult?.StatusCode ?? 500, error);
                }

               var deleteProductELasticSearch = await _elasticsearchService.DeleteAsync(productId);
                if (deleteProductELasticSearch == null || !deleteProductELasticSearch.Success)
                {
                    await _unitOfWork.AbortAsync();
                    var error = deleteProductELasticSearch?.Errors != null
                        ? string.Join(", ", deleteProductELasticSearch.Errors)
                        : "Something went wrong ";

                    return OperationResult<ProductDTO>.FailureResult(deleteProductELasticSearch?.StatusCode ?? 500, error);
                }

                await _unitOfWork.CommitAsync();
                var products2 = await _productCache.GetMemberListAsync(cacheKey);

                if (products2 != null)
                {
                    var index = products2.FindIndex(p => p.Id == parsedProductId);

                    if (index != -1)
                    {
                        products2.RemoveAt(index);
                        await _productCache.UpdateMemberListAsync(cacheKey, products2);
                    }
                }
                


                return OperationResult<ProductDTO>.SuccessResult(null);
            }
            catch (Exception ex)
            {
                await _unitOfWork.AbortAsync();

                return OperationResult<ProductDTO>.FailureResult(500, ex.Message);
            }
        }
  


    }

}