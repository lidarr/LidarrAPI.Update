﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LidarrAPI.Database;
using LidarrAPI.Database.Models;
using LidarrAPI.Update;
using LidarrAPI.Update.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Architecture = System.Runtime.InteropServices.Architecture;
using OperatingSystem = LidarrAPI.Update.OperatingSystem;

namespace LidarrAPI.Controllers
{
    [Route("v1/[controller]")]
    public class UpdateController : Controller
    {
        private readonly DatabaseContext _database;

        public UpdateController(DatabaseContext database)
        {
            _database = database;
        }

        [Route("{branch}/changes")]
        [HttpGet]
        public object GetChanges([FromRoute(Name = "branch")] Branch updateBranch,
            [FromQuery(Name = "version")] string urlVersion, [FromQuery(Name = "os")] OperatingSystem operatingSystem)
        {
            var updates = _database.UpdateEntities
                .Include(x => x.UpdateFiles)
                .Where(x => x.Branch == updateBranch &&
                       x.UpdateFiles.Any(u => u.OperatingSystem == operatingSystem))
                .OrderByDescending(x => x.ReleaseDate)
                .Take(5);

            var response = new List<UpdatePackage>();

            foreach (var update in updates)
            {
                var updateFile = update.UpdateFiles.FirstOrDefault(u => u.OperatingSystem == operatingSystem);
                if (updateFile == null) continue;

                UpdateChanges updateChanges = null;

                if (update.New.Count != 0 || update.Fixed.Count != 0)
                {
                    updateChanges = new UpdateChanges
                    {
                        New = update.New,
                        Fixed = update.Fixed
                    };
                }

                response.Add(new UpdatePackage
                    {
                        Version = update.Version,
                        ReleaseDate = update.ReleaseDate,
                        Filename = updateFile.Filename,
                        Url = updateFile.Url,
                        Changes = updateChanges,
                        Hash = updateFile.Hash,
                        Status = update.Status,
                        Branch = update.Branch.ToString().ToLower()
                    });
            }

            return response;
        }

        [Route("{branch}")]
        [HttpGet]
        public object GetUpdates([FromRoute(Name = "branch")] Branch updateBranch,
                                 [FromQuery(Name = "version")] string urlVersion,
                                 [FromQuery(Name = "os")] OperatingSystem operatingSystem,
                                 [FromQuery(Name = "runtime")] Runtime runtime,
                                 [FromQuery(Name = "arch")] Architecture arch)
        {
            // Check given version
            if (!Version.TryParse(urlVersion, out Version version))
            {
                return new
                {
                    ErrorMessage = "Invalid version number specified."
                };
            }

            // Mono and Dotnet are equivalent for our purposes
            if (runtime == Runtime.Mono)
            {
                runtime = Runtime.DotNet;
            }

            // If runtime is DotNet then default arch to x64
            if (runtime == Runtime.DotNet)
            {
                arch = Architecture.X64;
            }

            Expression<Func<UpdateFileEntity, bool>> predicate;
            if (operatingSystem == OperatingSystem.Linux)
            {
                predicate = x => x.OperatingSystem == operatingSystem && x.Architecture == arch && x.Runtime == runtime;
            }
            else
            {
                predicate = x => x.OperatingSystem == operatingSystem;
            }

            // Grab latest update based on branch and operatingsystem
            var update = _database.UpdateEntities
                .Include(x => x.UpdateFiles)
                .Where(x => x.Branch == updateBranch &&
                       x.UpdateFiles.AsQueryable().Any(predicate))
                .OrderByDescending(x => x.ReleaseDate)
                .FirstOrDefault();

            if (update == null)
            {
                return new
                    {
                        ErrorMessage = "Latest update not found."
                    };
            }

            // Check if update file is present
            var updateFile = update.UpdateFiles.FirstOrDefault(predicate.Compile());
            if (updateFile == null)
            {
                return new
                    {
                        ErrorMessage = "Latest update file not found."
                    };
            }

            // Compare given version and update version
            var updateVersion = new Version(update.Version);
            if (updateVersion.CompareTo(version) <= 0)
            {
                return new UpdatePackageContainer
                {
                    Available = false
                };
            }

            // Get the update changes
            UpdateChanges updateChanges = null;

            if (update.New.Count != 0 || update.Fixed.Count != 0)
            {
                updateChanges = new UpdateChanges
                {
                    New = update.New,
                    Fixed = update.Fixed
                };
            }

            return new UpdatePackageContainer
            {
                Available = true,
                UpdatePackage = new UpdatePackage
                {
                    Version = update.Version,
                    ReleaseDate = update.ReleaseDate,
                    Filename = updateFile.Filename,
                    Url = updateFile.Url,
                    Changes = updateChanges,
                    Hash = updateFile.Hash,
                    Branch = update.Branch.ToString().ToLower(),
                    Runtime = updateFile.Runtime.ToString().ToLower()
                }
            };
        }
    }
}
