﻿using DcslGs.Template.Common.Application.Commands;

namespace DcslGs.Template.Application.Commands.Company;

public record CompanyEditCommand : CommandBase
{
    protected CompanyEditCommand() {}

    public CompanyEditCommand(Guid id,
                              string name,
                              string email,
                              string webSiteUrl,
                              string address,
                              string city,
                              string county,
                              string postCode,
                              Guid countryId) : base(id)
    {
        Name = name;
        Email = email;
        WebSiteUrl = webSiteUrl;
        Address = address;
        City = city;
        County = county;
        PostCode = postCode;
        CountryId = countryId;
    }

    public string Name { get; }
    public string Email { get; }
    public string WebSiteUrl { get; }
    public string Address { get; }
    public string City { get; }
    public string County { get; }
    public string PostCode { get; }
    public Guid CountryId { get; }
}