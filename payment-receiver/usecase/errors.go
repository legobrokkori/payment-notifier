// usecase/errors.go
package usecase

import "errors"

var ErrDuplicateEvent = errors.New("event already processed")
