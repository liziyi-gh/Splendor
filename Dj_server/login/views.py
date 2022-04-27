import json

from django.http import HttpResponse
from django.http.response import HttpResponseBadRequest
from django.views.decorators.csrf import csrf_exempt
from django.shortcuts import render
from django.db import models

# class Users(models.Model):
#     pass


@csrf_exempt
def index(request):
    dict = {
        "loginFeedback": True,
    }
    print(f"request is {request.body}")
    # TODO: get from databases
    body = request.body.decode()
    if request.method == 'POST':
        return HttpResponse(json.dumps(dict))

    return HttpResponse(json.dumps(dict))


# Create your views here.
