﻿using AutoFixture;
using FluentAssertions;
using Monaco.Template.Backend.Application.Features.Product;
using Monaco.Template.Backend.Application.Infrastructure.Context;
using Monaco.Template.Backend.Application.Services.Contracts;
using Monaco.Template.Backend.Common.Tests;
using Monaco.Template.Backend.Domain.Model;
using Monaco.Template.Backend.Domain.Tests.Factories;
using Moq;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Monaco.Template.Backend.Application.Tests.Features.Product;

[ExcludeFromCodeCoverage]
[Trait("Application Commands - Product", "Edit")]
public class EditProductHandlerTests
{
	private readonly Mock<AppDbContext> _dbContextMock = new();
	private readonly Mock<IFileService> _fileServiceMock = new();
	private static readonly EditProduct.Command Command;

	static EditProductHandlerTests()
	{
		var fixture = new Fixture();
		Command = new(fixture.Create<Guid>(),		// Id
					  fixture.Create<string>(),		// Title
					  fixture.Create<string>(),		// Description
					  fixture.Create<decimal>(),	// Price
					  fixture.Create<Guid>(),		// CompanyId
					  fixture.Create<Guid[]>(),		// Pictures
					  fixture.Create<Guid>());		// DefaultPictureId
	}
	
	[Theory(DisplayName = "Edit existing Product succeeds")]
	[AutoDomainData(true)]
	public async Task CreateNewProductSucceeds(Domain.Model.Company company,
											   Image[] pictures)
	{
		_dbContextMock.CreateEntityMockAndSetupDbSetMock<AppDbContext, Domain.Model.Product>(out var productMock)
					  .CreateAndSetupDbSetMock(company, out var companyDbSetMock)
					  .CreateAndSetupDbSetMock(pictures);
		companyDbSetMock.Setup(x => x.FindAsync(It.IsAny<object?[]?>(),
												It.IsAny<CancellationToken>()))
						.ReturnsAsync(company);
		productMock.SetupGet(x => x.Pictures)
				   .Returns(pictures);
		productMock.SetupGet(x => x.Company)
				   .Returns(company);

		var product = productMock.Object;

		var command = Command with
					  {
						  Id = product.Id,
						  CompanyId = product.Company.Id,
						  Pictures = pictures.Select(x => x.Id)
											 .ToArray(),
						  DefaultPictureId = pictures.First()
													 .Id
					  };

		var sut = new EditProduct.Handler(_dbContextMock.Object, _fileServiceMock.Object);
		var result = await sut.Handle(command, new CancellationToken());

		productMock.Verify(x => x.Update(It.IsAny<string>(),
										 It.IsAny<string>(),
										 It.IsAny<decimal>()));
		_dbContextMock.Verify(x => x.SaveEntitiesAsync(It.IsAny<CancellationToken>()),
							  Times.Once);
		_fileServiceMock.Verify(x => x.DeleteImagesAsync(It.IsAny<Image[]>(),
														 It.IsAny<CancellationToken>()),
								Times.Once);
		_fileServiceMock.Verify(x => x.MakePermanentImagesAsync(It.IsAny<Image[]>(), 
																It.IsAny<CancellationToken>()),
								Times.Once);

		result.ValidationResult
			  .IsValid
			  .Should()
			  .BeTrue();
		result.ItemNotFound
			  .Should()
			  .BeFalse();
	}
}