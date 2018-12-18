<template>
  <div align="left" id="app">
    <div align="center" >
      <img v-if="searchResults.length == 0" @click="currentItem= null" src="./assets/logo.png">
    </div>
    <div class="container margin-top margin-bottom">
        <div class="row margin-bottom-small">
            <div class="col-md-1">
                <img v-show="searchResults.length > 0" v-bind:class="{ smallImge: searchResults.length>0 }" @click="currentItem= null" src="./assets/logo.png">
            </div>
            <div v-if="!currentItem" class="col-md-offset-2 col-md-6">
                <form >
                    <div class="input-group">
                        <input class="form-control" v-model="query" autofocus type="search" placeholder="Search for..." @input="onChangeAuto">
                        <span class="input-group-btn">
                            <button class="btn btn-default btn-search" type="submit" @click="search(query)"><i class="glyphicon glyphicon-search"></i></button>
                        </span>
                    </div><!-- /input-group -->
                </form>
            </div>
        </div>
        <div v-if="!currentItem" class="row">
            <div class="col-md-offset-3 col-md-6">
                <div class="autocomplete" v-show= "results.length > 0" >
                    <h4>Did you mean?</h4>
                    <ul class="suggestion-list">
                        <li class="autocomplete-result"  v-for="result in results" @click="search(result.name)">{{result.name}}</li>
                    </ul>
                </div>
                <h4 v-show="searchResults.length > 0">Found <b>{{searchResults.length}}</b> posts in <b>{{took}}</b> ms.</h4>
                <h4 v-show="message">{{message}}</h4>
            </div>
        </div>
        <div v-if="!currentItem" class="row" style="padding-bottom: 50px">
        </div>
        <div v-if="!currentItem" class="row">
            <div class="col-sm-2 col-xs-3">
                <div v-show="total > 0">
                    <h4 class="cursorPointer" @click="removeAllTags">Posts by tag</h4>
                    <ul class="category-list">
                        <li v-for="(count, tag) in aggs">
                            <span >
                                <a class="label label-primary vue !important" v-bind:class="{ selectedTag: activeFilters.indexOf(tag) != -1 }" @click="toggleFilters(tag)">{{tag}}</a>
                            </span><small class="pull-right">({{count}})</small>
                        </li>
                    </ul>
                </div>
            </div>
            <div class="col-xs-9 col-sm-offset-1">
                <div class="row margin-bottom-small" v-for="item in searchResults" v-show="!isLoading">
                    <div class="col-md-12" style="padding-bottom: 20px">
                        <div class="row ">
                            <div class="col-md-12">
                                <a class="cursorPointer" @click="selectPost(item.id)">
                                    <h3>{{item.title}}</h3>
                                </a>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <p>{{item.body.substring(0,300)}}</p>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-8">
                                <span v-for="tag in item.tags" class="label label-primary vue !important">{{tag}}</span>
                            </div>
                            <div class="col-md-4 text-right">
                                <small>Votes: <span class="label label-info label-round vue !important">{{item.score}}</span></small>
                                <small>Answers: <span class="label label-info label-round vue !important">{{item.answerCount}}</span></small>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div v-if="currentItem">
            <div class="col-xs-9 col-sm-offset-1">
                <div class="row margin-bottom-small">
                    <div class="col-md-12" style="padding-bottom: 20px">
                        <div class="row ">
                            <div class="col-md-12">
                                <a class="cursorPointer" @click="selectPost(currentItem.id)">
                                    <h3>{{currentItem.title}}</h3>
                                </a>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <p>{{currentItem.body}}</p>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-8">
                                <span v-for="tag in currentItem.tags" class="label label-primary vue !important">{{tag}}</span>
                            </div>
                            <div class="col-md-4 text-right">
                                <small>Votes: <span class="label label-info label-round vue !important">{{currentItem.score}}</span></small>
                                <small>Answers: <span class="label label-info label-round vue !important">{{currentItem.answerCount}}</span></small>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div style="padding-top: 15%;text-align:center;" class="col-xs-12">
            <div class="row"><h2>Similar Stories</h2></div>
                <div style="padding-top: 5%" class="col-xs-12">
                    <div class="margin-bottom-small" v-for="similar in csimilar">
                        <div class="col-md-3" style="padding-bottom: 20px;width:32%">
                            <div class="row ">
                                <div class="col-md-12">
                                    <a class="cursorPointer" @click="selectPost(similar.id)">
                                        <h3>{{similar.title}}</h3>
                                    </a>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-8">
                                    <span v-for="tag in similar.tags" class="label label-primary vue !important">{{tag}}</span>
                                </div>
                                <div class="col-md-4 text-right">
                                    <small>Votes: <span class="label label-info label-round vue !important">{{similar.score}}</span></small>
                                    <small>Answers: <span class="label label-info label-round vue !important">{{similar.answerCount}}</span></small>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
  </div>
</template>

<script>
import Vue from 'vue'
import axios from 'axios'
import VueAxios from 'vue-axios'

Vue.use(VueAxios, axios)
export default {
  name: 'App',
  components: {
    // Add a reference to the TodoList component in the components property
  },
  data() {
    return {
      results: [],
      activeFilters: [],
      searchResults: [],
      activeFilters: [],
      message: "",
      query: "",
      total: 0,
      aggs: {},
      took: 0,
      isLoading: false,
      currentItem: null,
      csimilar: []

    };
  },
  watch:{
      query: function(){
          if(this.query.length == 0)
          this.results=[];
      }
  },
  methods: {
    isActive : function (tag) {
        return activeFilters.length > 0 && activeFilters.indexOf(tag) >= 0;
    },
    onChangeAuto: function(){
        var self =this;
        const endpoint = 'http://localhost:5002/api/vizar-suggests?keyword=' +  this.query;
        if(this.query.length >0)
      axios.get(endpoint,{ headers: { 'Access-Control-Allow-Origin': true }} ).then(function(response){
                            self.results = response.data.result.suggests;
                        });
    },
    search: function(query){
        var self =this;
        self.query=query;
      axios.get('http://localhost:5002/api/vizar-suggests/search?keyword=' +  query).then(function(response){
                            self.results = [];
                            self.searchResults = response.data.results;
                            self.aggs = response.data.aggregationsByTags;
                            if (self.searchResults.length == 0)
                                self.message = "no results";
                            else 
                                self.total = response.data.total;
                            self.took = response.data.elapsedMilliseconds;
                            self.isLoading = false;
                        });
    },
    toggleFilters: function(tag){
        this.isLoading = true;
            var index = this.activeFilters.indexOf(tag);
            if (index === -1) {
                this.activeFilters.push(tag);
            } else {
                this.activeFilters.splice(index, 1);
            }

            if (this.activeFilters.length > 0) {
                this.searchByCategory();
            } else {
                this.search(tag);
            }
    },
    selectPost: function (id) {
        var self = this;
        self.findPost(id).then(function (response) {
            self.currentItem = response.data;
            self.moreLikeThis(id);
        });
    },
    findPost: function (id) {
        return axios.get('http://localhost:5002/api/vizar-suggests/searchbyid?id=' + id);
    },
    moreLikeThis: function (id) {
            var self = this;
            axios.get('http://localhost:5002/api/vizar-suggests/morelikethis?id=' + id).then(function (response) {
                self.csimilar = response.data.results;
            });
    },
    searchByCategory: function(){
        var self = this;
        var json = 
        {
            "q": self.query, 
            "categories": self.activeFilters 
        }
            axios.post('http://localhost:5002/api/vizar-suggests/searchbycategory',json).then(function (response) {
                self.isLoading = false;
                self.searchResults = response.data.results;
                self.aggs = response.data.aggregationsByTags;
                self.total = 0;
                if (response.data.results.length === 0)
                    self.message = "no results";
                else {
                    self.total = response.data.total;
                    self.took = response.data.elapsedMilliseconds;
                }
            });
            self.getSuggestions(this.query).then(function (response) {
                self.results = response.data.result;
            });
    },
    getSuggestions: function(id){
        return axios.get('http://localhost:5002/api/vizar-suggests/suggest?q=' + id);
    },
    removeAllTags: function(){
        var self = this;
        self.activeFilters = [];
        self.search(self.query);
    }
  }

};
</script>

<style>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #34495E;
  margin-top: 60px;
  padding-bottom: 100px;
}
h3,h4,h5{
    color: #41B883 !important;
}
.vue{
    color: white !important;
    background-color: #34495E;
}
body {
  -webkit-background-size: cover;
  -moz-background-size: cover;
  -o-background-size: cover;
  background-size: cover;
  font-family: 'Roboto', Tahoma, Arial, sans-serif;
  line-height: 1.5;
  font-size: 13px;
}
.selectedTag{
    background-color: #41B883 !important;
}
.autocomplete {
    position: absolute;
    width: 88%;
  }

  .autocomplete-results {
    padding: 0;
    margin: 0;
    border: 2px solid #eeeeee;
    background-color: black;
    height: 100%;
    overflow: hidden;
  }

  .autocomplete-result {
    list-style: none;
    text-align: left;
    color: #34495E;
    padding: 4px 2px;
    font-size: 15px;
    cursor: pointer;
  }

  .autocomplete-result:hover {
    background-color: #41B883;
    color: azure !important;
  }
  .cursorPointer{
      cursor: pointer;
  }
.smallImge{
    height: 75%;
    width: 75%;
    cursor: pointer;
}
.suggestion-list {
  background-color: rgba(255, 255, 255, 0.95);
  border: 1px solid #ddd;
  list-style: none;
  display: block;
  padding: 4px 2px;
  margin: 0;
  width: 100%;
  overflow: hidden;
  position: absolute;
  top: 20px;
  left: 0;
  z-index: 2;
}
</style>
