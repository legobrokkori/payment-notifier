// infrastructure/errors.go
package infrastructure

import "errors"

var ErrDuplicateKey = errors.New("duplicate key constraint violation")
