﻿// (c) Copyright Ascensio System SIA 2010-2022
//
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
//
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
//
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
//
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
//
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
//
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

namespace ASC.Api.Core.Core;

public class CustomEndpointDataSource : EndpointDataSource
{
    public EndpointDataSource Source { get; }
    public override IReadOnlyList<Endpoint> Endpoints { get; }

    public CustomEndpointDataSource(EndpointDataSource source)
    {
        Source = source;
        var endpoints = Source.Endpoints.Cast<RouteEndpoint>();
        Endpoints = endpoints
            .SelectMany(r =>
            {
                var endpoints = new List<RouteEndpoint>();

                var attr = r.Metadata.OfType<CustomHttpMethodAttribute>().FirstOrDefault();
                var constraintRouteAttr = r.Metadata.OfType<ConstraintRoute>().FirstOrDefault();
                var enableFormat = attr == null || !attr.DisableFormat;

                if (r.RoutePattern.Parameters.Any() && constraintRouteAttr != null)
                {
                    var routeValueDictionary = new RouteValueDictionary
                    {
                        { r.RoutePattern.Parameters.FirstOrDefault().Name, constraintRouteAttr.GetRouteConstraint() }
                    };

                    AddEndpoints(r.RoutePattern.Defaults, routeValueDictionary);

                }
                else
                {
                    AddEndpoints();
                }

                return endpoints;

                void AddEndpoints(IReadOnlyDictionary<string, object> defaults = null, RouteValueDictionary policies = null)
                {
                    endpoints.Add(new RouteEndpoint(r.RequestDelegate, RoutePatternFactory.Parse(r.RoutePattern.RawText, defaults, policies), r.Order, r.Metadata, r.DisplayName));

                    if (enableFormat)
                    {
                        endpoints.Add(new RouteEndpoint(r.RequestDelegate, RoutePatternFactory.Parse(r.RoutePattern.RawText + ".{format}", defaults, policies), r.Order, r.Metadata, r.DisplayName));
                    }
                    else
                    {
                        endpoints.Add(new RouteEndpoint(r.RequestDelegate, RoutePatternFactory.Parse(r.RoutePattern.RawText + ".json", defaults, policies), r.Order - 1, r.Metadata, r.DisplayName));
                        endpoints.Add(new RouteEndpoint(r.RequestDelegate, RoutePatternFactory.Parse(r.RoutePattern.RawText + ".xml", defaults, policies), r.Order - 1, r.Metadata, r.DisplayName));
                    }
                }

            }).ToList();
    }

    public override IChangeToken GetChangeToken()
    {
        return Source.GetChangeToken();
    }
}

public static class EndpointExtension
{
    public static IEndpointRouteBuilder MapCustom(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        var sources = endpoints.DataSources.First();
        endpoints.DataSources.Clear();
        endpoints.DataSources.Add(new CustomEndpointDataSource(sources));

        return endpoints;
    }
}