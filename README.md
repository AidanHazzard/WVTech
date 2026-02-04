# Willamette Valley Tech
A senior project from Western Oregon University by Natalie, Cooper, Easton, and Aidan

[About Us](/docs/team_info.md)<br>
[Our Vision](/docs/vision_statement.md)

## Milestone 4

[Data Model](docs/images/mp_data_model_v2.svg)

### Architectural Decisions 

#### .NET

We will be using .NET 10 for our project.

#### Testing

We are using NUnit for our testing framework for unit testing. BDD testing will be completed using [Reqnroll](https://reqnroll.net/) with NUnit for automated testing.

Javascript testing will be completed using [Jest](https://jestjs.io/).

#### Database

We will use EF Core Migrations to create the database and update schema from models in our project.

#### Authentication

We will use ASP.NET Core Identity to implement authentication

#### Styling

Bootstrap will be used to style the app

#### JavaScript

We will be using JQuery for DOM manipulation.

#### PR Practices

Pull requests will be merged into dev as soon as the maintainer is available. The maintainer will regularly check and review new PRs twice a day, in the morning and at night. The maintainer will also review a PR upon receiving notification from the submitter as soon as available.

PRs should be free of merge conflicts and submitted before 8 PM on the final Sunday of a sprint.

#### Project Folder Structure

```
WVTech
├───docs
├───src
│   └───MealPlanner
│       ├───MealPlanner
│       ├───MealPlanner.IntegrationTests
│       ├───MealPlanner.JSTests
│       └───MealPlanner.Tests
└───team
```