/****** Script for SelectTopNRows command from SSMS  ******/
SELECT *
  FROM [dbo].[Lookups]
  where category = 'transportmethod'

update lookups set [key] = 'painting_rollerbrush' where id = 61
update lookups set [key] = 'painting_spray' where id = 62
update lookups set [key] = 'drywall' where id = 63
update lookups set [key] = 'painting_roller' where id = 64
update lookups set [key] = 'painting_roller' where id = 65
update lookups set [key] = 'painting_roller' where id = 66
update lookups set [key] = 'painting_roller' where id = 67
update lookups set [key] = 'painting_roller' where id = 68


update lookups set [key] = 'transport_bus' where id = 29
update lookups set [key] = 'transport_car' where id = 30
update lookups set [key] = 'transport_pickup' where id = 31
update lookups set [key] = 'transport_van' where id = 32

update configs set publicConfig = 0 where category = 'Emails'
update configs set publicConfig = 0 where [key] = 'PayPalAccountID'

select * from configs

select * from employers where email = 'jciispam@gmail.com'
update employers set onlineSigninID = '1F6D3643-B6DE-4951-A0A6-F63740D45A46' where email = 'jciispam@gmail.com' 
delete from employers where id = 46770
select * from employers where onlineSigninId = '1F6D3643-B6DE-4951-A0A6-F63740D45A46'