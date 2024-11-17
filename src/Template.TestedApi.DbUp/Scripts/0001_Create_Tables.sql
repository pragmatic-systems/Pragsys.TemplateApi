-- Load Extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create Tables

CREATE TABLE todo_list ( 
  item_id uuid NOT NULL DEFAULT uuid_generate_v4 (),
  title VARCHAR(128) NOT NULL,
  description VARCHAR(512) NOT NULL,
  due_date date NOT NULL,
  open boolean default true,
  closed_date date default NULL,
  PRIMARY KEY(item_id)
  );