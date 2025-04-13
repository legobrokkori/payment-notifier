FROM golang:1.24.2 AS build

WORKDIR /app
COPY . .

RUN go mod tidy
RUN go build -o main ./cmd

FROM golang:1.24.2 AS run
WORKDIR /app
COPY --from=build /app/main .

EXPOSE 8080
CMD ["./main"]