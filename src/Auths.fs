namespace Zrpg.Auths

type Password = | Password of string
type Username = string

type AuthCmd =
  | PostCredentials of Username * Password