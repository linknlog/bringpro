# bringpro - Code Samples

Some samples of my work from the bringpro project.

http://bringpro-dev.azurewebsites.net/

bringpro is a same day, on-demand delivery service that picks up,
delivers and installs large items. bringpro is a profitable Orange County
company currently expanding to Los Angeles.
Worked with a team of 10 other developers to implement architecture
for a new admin dashboard plus several user facing websites used to
coordinate logistics, process payments, and provide analytics and
reporting. Built on ASP.Net MVC C# stack, using MSSQL for the
database, Angular, and material design specification for the front end.


# Customer Order Form

http://bringpro-dev.azurewebsites.net/bringpro/job#/

The original order form was 8 pages, not mobile friendly, and with only one or two actions per page.
This caused the order process to be very long and hard to read on mobile devices.
I designed a new layout and used Angular 1.6, HTML, and CSS to redesign a mobile friendly order form using accordions.
Each accordion panel was either closed or hidden until the current accordion was complete and passed validation.
Each new job waypoint was cloned using ng-repeat and ng-model was used to track each individual accordion panel.
I created custom APIs to handle the job way points, job scheduling, and items to be picked up or delivered. As well as custom SQL stored procedures. Dropzone was also added so customers can upload custom images of their items.


# Admin User Section

The admin user section allowed the admin to do several things. They could create users, edit users, set roles for different users,
search for users, and delete users. The view was accomplished using HTML, Angular, and CSS. Pagination was done server side using SQL. Custom APIs and services were also written to accomplish all the database calls.







