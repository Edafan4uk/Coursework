# Travelling blog
Website for travellers
* Full-stack web app based on **Asp.net Core 2.1, Angular6 and MS Sql Server**
* Authentication, role-based and claim-based authorization via **JWT** and **IdentityServer4**. Facebook **OAuth2** authentication
* Rest API with multi layer architecture based on **Asp.net Core web API**
* Frontend application based on **Angular6, Bootstrap4 and RxJS**
* **MS Sql Server** along with **EF Core Code First** approach for DB

Repository structure
* *Coursework/AppForTravellers/TravelingBlog* - **asp.net core web api project**
  * Contains controllers, action filters and extensions classes
* *Coursework/AppForTravellers/TravelingBlog.Angular* - **Angular6 application**
  * Split into logically separated modules(e.g. account, admin, home...) along with their components, services and models.
* *Coursework/AppForTravellers/XUnitTest* - **api unit tests project**
* *Coursework/AppForTravellers/TravelingBlog.DataAcceesLayer* - **DAL**
  * Contains IdentityDbContext class along with EF Core Code First domain models and their configuration classes. Repository & UoF
* *Coursework/AppForTravellers/TravelingBlog.BusinessLogicLayer* - **Services**
  * Contains interfaces along with their implementations for services which perform some business logic
* *Coursework/AppForTravellers/TravelingBlog.Helpers* - **utility classes**
